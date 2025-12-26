using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	/// <summary>
	/// 快速导航路径项
	/// </summary>
	public partial class QuickPathItem : ObservableObject
	{
		[ObservableProperty]
		private string _path = string.Empty;

		[ObservableProperty]
		private string _displayName = string.Empty;

		/// <summary>
		/// 最大显示字符数
		/// </summary>
		private const int MaxDisplayLength = 35;

		public QuickPathItem(string path)
		{
			Path = path;
			DisplayName = FormatPathHeadTail(path);
		}

		/// <summary>
		/// 将路径格式化为头尾显示格式：C:\User...\xxx\
		/// </summary>
		private static string FormatPathHeadTail(string path)
		{
			if (string.IsNullOrEmpty(path))
				return path;

			// 如果路径较短，直接显示完整路径
			if (path.Length <= MaxDisplayLength)
				return path;

			try
			{
				// 解析路径各部分
				var parts = path.Split(new[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar }, 
					StringSplitOptions.RemoveEmptyEntries);

				if (parts.Length == 0)
					return path;

				// 获取根目录（如 C:\)
				var root = parts[0] + System.IO.Path.DirectorySeparatorChar;

				// 获取最后一个文件夹名
				var lastFolder = parts[parts.Length - 1];

				if (parts.Length == 1)
				{
					// 只有根目录
					return root;
				}
				else if (parts.Length == 2)
				{
					// 只有两级，直接显示
					return path;
				}
				else
				{
					// 头尾显示格式：C:\...\lastFolder
					return $"{root}...{System.IO.Path.DirectorySeparatorChar}{lastFolder}";
				}
			}
			catch
			{
				// 解析失败时返回原始路径截断
				return path.Length > MaxDisplayLength 
					? path.Substring(0, MaxDisplayLength - 3) + "..."
					: path;
			}
		}
	}

	/// <summary>
	/// 文件系统节点模型 - 用于树形结构显示
	/// </summary>
	public partial class FileSystemNode : ObservableObject
	{
		[ObservableProperty]
		private string _name = string.Empty;

		[ObservableProperty]
		private string _fullPath = string.Empty;

		[ObservableProperty]
		private bool _isDirectory;

		private bool _isExpanded;
		/// <summary>
		/// 节点展开状态 - 展开时自动加载子节点
		/// </summary>
		public bool IsExpanded
		{
			get => _isExpanded;
			set
			{
				if (SetProperty(ref _isExpanded, value) && value)
				{
					// 展开时自动加载子节点
					LoadChildren();
				}
			}
		}

		[ObservableProperty]
		private bool _isLoaded;

		[ObservableProperty]
		private string _iconKey = "📄";

		[ObservableProperty]
		private ObservableCollection<FileSystemNode> _children = new();

		/// <summary>
		/// 文件大小（仅文件有效）
		/// </summary>
		[ObservableProperty]
		private string _sizeDisplay = string.Empty;

		/// <summary>
		/// 最后修改时间
		/// </summary>
		[ObservableProperty]
		private string _lastModified = string.Empty;

		/// <summary>
		/// 是否为驱动器根节点
		/// </summary>
		public bool IsDrive { get; set; }

		/// <summary>
		/// 加载子节点（延迟加载）
		/// </summary>
		public void LoadChildren()
		{
			if (IsLoaded || !IsDirectory)
				return;

			try
			{
				Children.Clear();

				var dirInfo = new DirectoryInfo(FullPath);

				// 先加载子目录
				foreach (var dir in dirInfo.GetDirectories().OrderBy(d => d.Name))
				{
					try
					{
						// 跳过系统和隐藏文件夹
						if ((dir.Attributes & FileAttributes.Hidden) != 0 ||
							(dir.Attributes & FileAttributes.System) != 0)
							continue;

						var node = new FileSystemNode
						{
							Name = dir.Name,
							FullPath = dir.FullName,
							IsDirectory = true,
							IconKey = "📁",
							LastModified = dir.LastWriteTime.ToString("yyyy-MM-dd HH:mm")
						};

						// 添加占位符以支持展开
						node.Children.Add(new FileSystemNode { Name = "加载中...", IconKey = "⏳" });
						Children.Add(node);
					}
					catch
					{
						// 跳过无权限访问的目录
					}
				}

				// 再加载文件
				foreach (var file in dirInfo.GetFiles().OrderBy(f => f.Name))
				{
					try
					{
						// 跳过隐藏文件
						if ((file.Attributes & FileAttributes.Hidden) != 0)
							continue;

						var node = new FileSystemNode
						{
							Name = file.Name,
							FullPath = file.FullName,
							IsDirectory = false,
							IconKey = GetFileIcon(file.Extension),
							SizeDisplay = FormatFileSize(file.Length),
							LastModified = file.LastWriteTime.ToString("yyyy-MM-dd HH:mm")
						};
						Children.Add(node);
					}
					catch
					{
						// 跳过无权限访问的文件
					}
				}

				IsLoaded = true;
			}
			catch (Exception)
			{
				// 访问失败时显示错误提示
				Children.Clear();
				Children.Add(new FileSystemNode { Name = "无法访问", IconKey = "⚠️" });
				IsLoaded = true;
			}
		}

		/// <summary>
		/// 根据文件扩展名获取图标
		/// </summary>
		private static string GetFileIcon(string extension)
		{
			return extension.ToLowerInvariant() switch
			{
				".las" => "📊",      // 测井曲线
				".sgy" or ".segy" => "🌊",  // 地震数据
				".shp" => "🗺️",     // 矢量图层
				".txt" or ".log" => "📝",   // 文本文件
				".pdf" => "📕",      // PDF
				".doc" or ".docx" => "📘",  // Word
				".xls" or ".xlsx" => "📗",  // Excel
				".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif" => "🖼️", // 图片
				".zip" or ".rar" or ".7z" => "📦", // 压缩包
				".exe" or ".dll" => "⚙️",   // 可执行文件
				".lmproj" => "📂",   // LithoMind项目文件
				_ => "📄"            // 默认文件图标
			};
		}

		/// <summary>
		/// 格式化文件大小显示
		/// </summary>
		private static string FormatFileSize(long bytes)
		{
			string[] sizes = { "B", "KB", "MB", "GB", "TB" };
			int order = 0;
			double size = bytes;
			while (size >= 1024 && order < sizes.Length - 1)
			{
				order++;
				size /= 1024;
			}
			return $"{size:0.##} {sizes[order]}";
		}
	}

	/// <summary>
	/// 本地文件目录视图模型
	/// 实现文件系统访问和树形结构显示
	/// </summary>
	public partial class LocalFilesViewModel : PageViewModelBase
	{
		/// <summary>
		/// 滚动到节点事件 - 通知View滚动TreeView到指定节点
		/// </summary>
		public event Action<FileSystemNode>? ScrollToNodeRequested;

		/// <summary>
		/// 文件选择事件 - 用于通知预览区域更新
		/// </summary>
		public event Action<FileSystemNode>? FileSelected;

		/// <summary>
		/// 根节点集合（驱动器列表）
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<FileSystemNode> _rootNodes = new();

		/// <summary>
		/// 当前选中的节点
		/// </summary>
		[ObservableProperty]
		private FileSystemNode? _selectedNode;

		/// <summary>
		/// 当前路径
		/// </summary>
		[ObservableProperty]
		private string _currentPath = string.Empty;

		/// <summary>
		/// 是否正在加载
		/// </summary>
		[ObservableProperty]
		private bool _isLoading;

		/// <summary>
		/// 快速导航路径集合
		/// </summary>
		[ObservableProperty]
		private ObservableCollection<QuickPathItem> _quickNavigationPaths = new();

		/// <summary>
		/// 是否有快速导航路径
		/// </summary>
		public bool HasQuickPaths => QuickNavigationPaths.Count > 0;

		/// <summary>
		/// 快速路径集合变化时通知UI
		/// </summary>
		partial void OnQuickNavigationPathsChanged(ObservableCollection<QuickPathItem> value)
		{
			OnPropertyChanged(nameof(HasQuickPaths));
		}

		public LocalFilesViewModel()
		{
			Id = "LocalFiles";
			Title = "本地文件";
			IconKey = "📁";
			Order = 1;

			// 初始化时加载驱动器列表
			LoadDrives();

			// 监听快速导航集合变化
			QuickNavigationPaths.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasQuickPaths));
		}

		/// <summary>
		/// 加载本地驱动器列表
		/// </summary>
		private void LoadDrives()
		{
			try
			{
				RootNodes.Clear();

				foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
				{
					var driveNode = new FileSystemNode
					{
						Name = $"{drive.Name} ({drive.VolumeLabel})",
						FullPath = drive.RootDirectory.FullName,
						IsDirectory = true,
						IsDrive = true,
						IconKey = drive.DriveType switch
						{
							DriveType.Fixed => "💾",
							DriveType.Removable => "💿",
							DriveType.Network => "🌐",
							DriveType.CDRom => "💿",
							_ => "📀"
						},
						SizeDisplay = $"{FormatSize(drive.AvailableFreeSpace)} 可用 / {FormatSize(drive.TotalSize)}"
					};

					// 添加占位符以支持展开
					driveNode.Children.Add(new FileSystemNode { Name = "加载中...", IconKey = "⏳" });
					RootNodes.Add(driveNode);
				}
			}
			catch (Exception)
			{
				RootNodes.Add(new FileSystemNode { Name = "无法获取驱动器列表", IconKey = "⚠️" });
			}
		}

		/// <summary>
		/// 格式化存储大小
		/// </summary>
		private static string FormatSize(long bytes)
		{
			string[] sizes = { "B", "KB", "MB", "GB", "TB" };
			int order = 0;
			double size = bytes;
			while (size >= 1024 && order < sizes.Length - 1)
			{
				order++;
				size /= 1024;
			}
			return $"{size:0.#} {sizes[order]}";
		}

		/// <summary>
		/// 刷新驱动器列表命令
		/// </summary>
		[RelayCommand]
		public void RefreshDrives()
		{
			LoadDrives();
		}

		/// <summary>
		/// 展开节点时加载子节点
		/// </summary>
		[RelayCommand]
		public void ExpandNode(FileSystemNode? node)
		{
			if (node == null || !node.IsDirectory)
				return;

			if (!node.IsLoaded)
			{
				node.LoadChildren();
			}
		}

		/// <summary>
		/// 选择节点
		/// </summary>
		[RelayCommand]
		public void SelectNode(FileSystemNode? node)
		{
			if (node == null)
				return;

			SelectedNode = node;
			CurrentPath = node.FullPath;

			// 如果是文件，触发文件选择事件
			if (!node.IsDirectory)
			{
				FileSelected?.Invoke(node);
			}
		}

		/// <summary>
		/// 导航到指定路径
		/// </summary>
		[RelayCommand]
		public void NavigateTo(string? path)
		{
			if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
				return;

			CurrentPath = path;
			// 可以在这里实现自动定位到树节点的逻辑
		}

		/// <summary>
		/// 添加文件夹到快速导航 - 打开文件夹选择对话框
		/// </summary>
		[RelayCommand]
		public async Task AddFolderToQuickNavAsync()
		{
			try
			{
				// 获取主窗口
				var topLevel = GetTopLevel();
				if (topLevel == null)
					return;

				// 打开文件夹选择对话框
				var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
				{
					Title = "选择要添加到快速导航的文件夹",
					AllowMultiple = false
				});

				if (folders.Count > 0)
				{
					var folder = folders[0];
					var folderPath = folder.Path.LocalPath;

					// 检查是否已存在
					if (!QuickNavigationPaths.Any(p => p.Path.Equals(folderPath, StringComparison.OrdinalIgnoreCase)))
					{
						QuickNavigationPaths.Add(new QuickPathItem(folderPath));
					}
				}
			}
			catch (Exception)
			{
				// 忽略错误
			}
		}

		/// <summary>
		/// 导航到快速路径并在树中展开
		/// </summary>
		[RelayCommand]
		public void NavigateToQuickPath(string? path)
		{
			if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
				return;

			CurrentPath = path;

			// 尝试在树中定位并展开该路径
			ExpandToPath(path);
		}

		/// <summary>
		/// 移除快速导航路径
		/// </summary>
		[RelayCommand]
		public void RemoveQuickPath(string? path)
		{
			if (string.IsNullOrEmpty(path))
				return;

			var item = QuickNavigationPaths.FirstOrDefault(p => p.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
			if (item != null)
			{
				QuickNavigationPaths.Remove(item);
			}
		}

		/// <summary>
		/// 在树中展开到指定路径
		/// </summary>
		private void ExpandToPath(string targetPath)
		{
			try
			{
				// 解析路径各级
				var pathParts = targetPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
					.Where(p => !string.IsNullOrEmpty(p)).ToList();

				if (pathParts.Count == 0)
					return;

				// 找到对应的驱动器根节点
				var driveLetter = pathParts[0] + Path.DirectorySeparatorChar;
				var driveNode = RootNodes.FirstOrDefault(n => 
					n.FullPath.StartsWith(driveLetter, StringComparison.OrdinalIgnoreCase));

				if (driveNode == null)
					return;

				// 展开驱动器
				driveNode.IsExpanded = true;

				// 逐级展开子节点
				var currentNode = driveNode;
				var currentPath = driveLetter;

				for (int i = 1; i < pathParts.Count; i++)
				{
					currentPath = Path.Combine(currentPath, pathParts[i]);
					var childNode = currentNode.Children.FirstOrDefault(n =>
						n.FullPath.Equals(currentPath, StringComparison.OrdinalIgnoreCase));

					if (childNode == null)
						break;

					childNode.IsExpanded = true;
					currentNode = childNode;
				}

				// 选中最后一个节点
				SelectedNode = currentNode;

				// 通知View滚动到该节点
				ScrollToNodeRequested?.Invoke(currentNode);
			}
			catch (Exception)
			{
				// 忽略展开错误
			}
		}

		/// <summary>
		/// 获取TopLevel窗口用于对话框
		/// </summary>
		private static TopLevel? GetTopLevel()
		{
			if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				return desktop.MainWindow;
			}
			return null;
		}
	}
}

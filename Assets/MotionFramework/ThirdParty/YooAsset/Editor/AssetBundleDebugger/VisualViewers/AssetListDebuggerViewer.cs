#if UNITY_2019_4_OR_NEWER
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
	internal class AssetListDebuggerViewer
	{
		private VisualTreeAsset _visualAsset;
		private TemplateContainer _root;

		private ListView _assetListView;
		private ListView _dependListView;
		private DebugSummy _summy;

		/// <summary>
		/// 初始化页面
		/// </summary>
		public void InitViewer()
		{
			// 加载布局文件
			string rootPath = EditorTools.GetYooAssetPath();
			string uxml = $"{rootPath}/Editor/AssetBundleDebugger/VisualViewers/AssetListDebuggerViewer.uxml";
			_visualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxml);
			if (_visualAsset == null)
			{
				Debug.LogError($"Not found {nameof(AssetListDebuggerViewer)}.uxml : {uxml}");
				return;
			}
			_root = _visualAsset.CloneTree();
			_root.style.flexGrow = 1f;

			// 资源列表
			_assetListView = _root.Q<ListView>("TopListView");
			_assetListView.makeItem = MakeAssetListViewItem;
			_assetListView.bindItem = BindAssetListViewItem;

#if UNITY_2020_1_OR_NEWER
			_assetListView.onSelectionChange += AssetListView_onSelectionChange;
#else
			_assetListView.onSelectionChanged += AssetListView_onSelectionChange;
#endif
			// 依赖列表
			_dependListView = _root.Q<ListView>("BottomListView");
			_dependListView.makeItem = MakeDependListViewItem;
			_dependListView.bindItem = BindDependListViewItem;
		}

		/// <summary>
		/// 填充页面数据
		/// </summary>
		public void FillViewData(DebugSummy summy, string searchKeyWord)
		{
			_summy = summy;
			_assetListView.Clear();
			_assetListView.itemsSource = FilterViewItems(summy, searchKeyWord);
		}
		private List<DebugProviderInfo> FilterViewItems(DebugSummy summy, string searchKeyWord)
		{
			var result = new List<DebugProviderInfo>(summy.ProviderInfos.Count);
			foreach (var providerInfo in summy.ProviderInfos)
			{
				if(string.IsNullOrEmpty(searchKeyWord) == false)
				{
					if (providerInfo.AssetPath.Contains(searchKeyWord) == false)
						continue;					
				}
				result.Add(providerInfo);
			}
			return result;
		}

		/// <summary>
		/// 挂接到父类页面上
		/// </summary>
		public void AttachParent(VisualElement parent)
		{
			parent.Add(_root);
		}

		/// <summary>
		/// 从父类页面脱离开
		/// </summary>
		public void DetachParent()
		{
			_root.RemoveFromHierarchy();
		}


		// 资源列表相关
		private VisualElement MakeAssetListViewItem()
		{
			VisualElement element = new VisualElement();
			element.style.flexDirection = FlexDirection.Row;

			{
				var label = new Label();
				label.name = "Label1";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				label.style.flexGrow = 1f;
				label.style.width = 280;
				element.Add(label);
			}

			{
				var label = new Label();
				label.name = "Label2";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				//label.style.flexGrow = 1f;
				label.style.width = 100;
				element.Add(label);
			}

			{
				var label = new Label();
				label.name = "Label3";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				//label.style.flexGrow = 1f;
				label.style.width = 120;
				element.Add(label);
			}

			return element;
		}
		private void BindAssetListViewItem(VisualElement element, int index)
		{
			var sourceData = _assetListView.itemsSource as List<DebugProviderInfo>;
			var providerInfo = sourceData[index];
			
			// Asset Path
			var label1 = element.Q<Label>("Label1");
			label1.text = providerInfo.AssetPath;

			// Ref Count
			var label2 = element.Q<Label>("Label2");
			label2.text = providerInfo.RefCount.ToString();

			// Status
			var label3 = element.Q<Label>("Label3");
			label3.text = providerInfo.Status.ToString();
		}
		private void AssetListView_onSelectionChange(IEnumerable<object> objs)
		{
			foreach (var item in objs)
			{
				DebugProviderInfo providerInfo = item as DebugProviderInfo;
				FillDependListView(providerInfo);
			}
		}

		// 依赖列表相关
		private VisualElement MakeDependListViewItem()
		{
			VisualElement element = new VisualElement();
			element.style.flexDirection = FlexDirection.Row;

			{
				var label = new Label();
				label.name = "Label1";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				label.style.flexGrow = 1f;
				label.style.width = 280;
				element.Add(label);
			}

			{
				var label = new Label();
				label.name = "Label2";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				//label.style.flexGrow = 1f;
				label.style.width = 100;
				element.Add(label);
			}

			{
				var label = new Label();
				label.name = "Label3";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				//label.style.flexGrow = 1f;
				label.style.width = 100;
				element.Add(label);
			}

			{
				var label = new Label();
				label.name = "Label4";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				//label.style.flexGrow = 1f;
				label.style.width = 120;
				element.Add(label);
			}

			return element;
		}
		private void BindDependListViewItem(VisualElement element, int index)
		{
			List<DebugBundleInfo> bundles = _dependListView.itemsSource as List<DebugBundleInfo>;
			DebugBundleInfo bundleInfo = bundles[index];

			// Bundle Name
			var label1 = element.Q<Label>("Label1");
			label1.text = bundleInfo.BundleName;

			// Version
			var label2 = element.Q<Label>("Label2");
			label2.text = bundleInfo.Version.ToString();

			// Ref Count
			var label3 = element.Q<Label>("Label3");
			label3.text = bundleInfo.RefCount.ToString();

			// Status
			var label4 = element.Q<Label>("Label4");
			label4.text = bundleInfo.Status.ToString();
		}
		private void FillDependListView(DebugProviderInfo providerInfo)
		{
			_dependListView.Clear();
#if UNITY_2020_1_OR_NEWER
			_dependListView.ClearSelection();
#endif
			_dependListView.itemsSource = providerInfo.BundleInfos;
		}
	}
}
#endif
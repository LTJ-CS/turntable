using System;
using UnityEngine.UIElements;
namespace Editor.Scripts
{
    /// <summary>
    /// Tab 的控制器
    /// </summary>
    public class TabbedMenuController
    {
        private const string tabClassName = "tab";
        private const string currentlySelectedTabClassName = "currentlySelectedTab";
        private const string unselectedContentClassName = "unselectedContent";
        
        private const string tabNameSuffix = "Tab";
        private const string contentNameSuffix = "Content";

        private readonly VisualElement root;
        
        public  TabbedMenuController(VisualElement root)
        {
            this.root = root;
        }

        public Action<string> OnTabSelected;

        public void RegisterTabCallbacks()
        {
            var tabs = GetAllTabs();
            tabs.ForEach(tab =>
            {
                tab.RegisterCallback<ClickEvent>(TabOnClick);
            });
        }
        
        private void TabOnClick(ClickEvent evt)
        {
            var clickedTab = evt.currentTarget as Label;
            if (!TabIsCurrentlySelected(clickedTab))
            {
                GetAllTabs().Where(
                        tab => tab != clickedTab && TabIsCurrentlySelected((tab)))
                    .ForEach(UnselectTab);

                SelectTab(clickedTab);
            }
        }
        private void SelectTab(Label tab)
        {
            tab.AddToClassList(currentlySelectedTabClassName);
            var content = FindContent(tab);
            content.RemoveFromClassList(unselectedContentClassName);
            
            OnTabSelected?.Invoke(tab.name);
        }
        
        private void UnselectTab(Label tab)
        {
            tab.RemoveFromClassList((currentlySelectedTabClassName));
            var content = FindContent(tab);
            content.AddToClassList(unselectedContentClassName);
        }
        
        private static bool TabIsCurrentlySelected(Label tab)
        {
            return tab.ClassListContains(currentlySelectedTabClassName);
        }

        private UQueryBuilder<Label> GetAllTabs()
        {
            return root.Query<Label>(className: tabClassName);
        }

        private static string GenerateContentName(Label tab) =>
            tab.name.Replace(tabNameSuffix, contentNameSuffix);

        private VisualElement FindContent(Label tab)
        {
            return root.Q(GenerateContentName(tab));
        }
    }
}
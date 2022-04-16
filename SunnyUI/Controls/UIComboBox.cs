﻿/******************************************************************************
 * SunnyUI 开源控件库、工具类库、扩展类库、多页面开发框架。
 * CopyRight (C) 2012-2022 ShenYongHua(沈永华).
 * QQ群：56829229 QQ：17612584 EMail：SunnyUI@QQ.Com
 *
 * Blog:   https://www.cnblogs.com/yhuse
 * Gitee:  https://gitee.com/yhuse/SunnyUI
 * GitHub: https://github.com/yhuse/SunnyUI
 *
 * SunnyUI.dll can be used for free under the GPL-3.0 license.
 * If you use this code, please keep this note.
 * 如果您使用此代码，请保留此说明。
 ******************************************************************************
 * 文件名称: UIComboBox.cs
 * 文件说明: 组合框
 * 当前版本: V3.1
 * 创建日期: 2020-01-01
 *
 * 2020-01-01: V2.2.0 增加文件说明
 * 2020-06-11: V2.2.5 增加DataSource，支持数据绑定
 * 2021-05-06: V3.0.3 解决鼠标下拉选择，触发SelectedIndexChanged两次的问题
 * 2021-06-03: V3.0.4 更新了数据绑定相关代码
 * 2021-08-03: V3.0.5 Items.Clear后清除显示
 * 2021-08-15: V3.0.6 重写了水印文字的画法，并增加水印文字颜色
 * 2022-01-16: V3.1.0 增加了下拉框颜色设置
 * 2022-04-13: V3.1.3 根据Text自动选中SelectIndex
 * 2022-04-15: V3.1.3 增加过滤
******************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace Sunny.UI
{
    [DefaultProperty("Items")]
    [DefaultEvent("SelectedIndexChanged")]
    [ToolboxItem(true)]
    [LookupBindingProperties("DataSource", "DisplayMember", "ValueMember", "SelectedValue")]
    public sealed partial class UIComboBox : UIDropControl, IToolTip
    {
        public UIComboBox()
        {
            InitializeComponent();
            ListBox.SelectedIndexChanged += Box_SelectedIndexChanged;
            ListBox.ValueMemberChanged += Box_ValueMemberChanged;
            ListBox.SelectedValueChanged += ListBox_SelectedValueChanged;
            ListBox.ItemsClear += ListBox_ItemsClear;
            ListBox.ItemsRemove += ListBox_ItemsRemove;

            filterForm.BeforeListClick += ListBox_Click;

            edit.TextChanged += Edit_TextChanged;
            edit.KeyDown += Edit_KeyDown;
            DropDownWidth = 150;
            fullControlSelect = true;
        }

        private void ListBox_Click(object sender, EventArgs e)
        {
            SelectTextChange = true;
            filterSelectedItem = filterList[(int)sender];
            filterSelectedValue = GetItemText(filterSelectedItem);
            Text = filterSelectedValue.ToString();
            edit.SelectionStart = Text.Length;
            SelectedValueChanged?.Invoke(this, EventArgs.Empty);
            SelectTextChange = false;
        }

        private void ShowDropDownFilter()
        {
            FilterItemForm.AutoClose = false;
            if (!FilterItemForm.Visible)
            {
                FilterItemForm.Show(this, new Size(DropDownWidth < Width ? Width : DropDownWidth, CalcItemFormHeight()));
                edit.Focus();
            }
        }

        private void Edit_KeyDown(object sender, KeyEventArgs e)
        {
            if (ShowFilter)
            {
                int cnt = filterForm.ListBox.Items.Count;
                int idx = filterForm.ListBox.SelectedIndex;

                if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up)
                {
                    ShowDropDownFilter();
                    if (cnt > 0)
                    {
                        if (e.KeyCode == Keys.Down)
                        {
                            if (idx < cnt - 1)
                                filterForm.ListBox.SelectedIndex++;
                        }

                        if (e.KeyCode == Keys.Up)
                        {
                            if (idx > 0)
                                filterForm.ListBox.SelectedIndex--;
                        }
                    }
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    FilterItemForm.Close();
                }
                else if (e.KeyCode == Keys.Return)
                {
                    if (FilterItemForm.Visible)
                    {
                        if (cnt > 0 && idx >= 0 && idx < cnt)
                        {
                            SelectTextChange = true;
                            filterSelectedItem = filterList[idx];
                            filterSelectedValue = GetItemText(filterSelectedItem);
                            Text = filterSelectedValue.ToString();
                            edit.SelectionStart = Text.Length;
                            SelectedValueChanged?.Invoke(this, EventArgs.Empty);
                            SelectTextChange = false;
                        }

                        FilterItemForm.Close();
                    }
                    else
                    {
                        ShowDropDownFilter();
                    }
                }
                else
                {
                    base.OnKeyDown(e);
                }
            }
            else
            {
                if (e.KeyCode == Keys.Enter)
                {
                    ShowDropDown();
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    ItemForm.Close();
                }
                else
                {
                    base.OnKeyDown(e);
                }
            }
        }

        private object filterSelectedItem;
        private object filterSelectedValue;
        private bool showFilter;

        [DefaultValue(false)]
        [Description("显示过滤"), Category("SunnyUI")]
        public bool ShowFilter
        {
            get => showFilter;
            set
            {
                showFilter = value;
                if (value)
                {
                    DropDownStyle = UIDropDownStyle.DropDown;
                }
            }
        }

        [DefaultValue(false)]
        [Description("过滤显示最大条目数"), Category("SunnyUI")]
        public int FilterMaxCount { get; set; } = 50;

        protected override void DropDownStyleChanged()
        {
            if (DropDownStyle == UIDropDownStyle.DropDownList)
            {
                showFilter = false;
            }
        }

        CurrencyManager dataManager;

        private void SetDataConnection()
        {
            if (DropDownStyle == UIDropDownStyle.DropDown && DataSource != null && DisplayMember.IsValid())
            {
                dataManager = (CurrencyManager)BindingContext[DataSource, new BindingMemberInfo(DisplayMember).BindingPath];
            }
        }

        public Control ExToolTipControl()
        {
            return edit;
        }

        [DefaultValue(false)]
        public bool Sorted
        {
            get => ListBox.Sorted;
            set => ListBox.Sorted = value;
        }

        public int FindString(string s)
        {
            return ListBox.FindString(s);
        }

        public int FindString(string s, int startIndex)
        {
            return ListBox.FindString(s, startIndex);
        }

        public int FindStringExact(string s)
        {
            return ListBox.FindStringExact(s);
        }

        public int FindStringExact(string s, int startIndex)
        {
            return ListBox.FindStringExact(s, startIndex);
        }

        private void ListBox_ItemsRemove(object sender, EventArgs e)
        {
            if (ListBox.Count == 0)
            {
                Text = "";
                edit.Text = "";
            }
        }

        private void ListBox_ItemsClear(object sender, EventArgs e)
        {
            Text = "";
            edit.Text = "";
        }

        public new event EventHandler TextChanged;

        private void Edit_TextChanged(object sender, EventArgs e)
        {
            TextChanged?.Invoke(this, e);
            if (DropDownStyle == UIDropDownStyle.DropDownList) return;

            if (!ShowFilter)
            {
                if (SelectTextChange) return;
                if (Text.IsValid())
                {
                    ListBox.ListBox.Text = Text;
                }
                else
                {
                    SelectTextChange = true;
                    SelectedIndex = -1;
                    edit.Text = "";
                    SelectTextChange = false;
                }
            }
            else
            {
                if (edit.Focused && Text.IsValid())
                {
                    ShowDropDownFilter();
                }

                filterForm.ListBox.Items.Clear();
                if (Text.IsValid())
                {
                    filterList.Clear();

                    if (DataSource == null)
                    {
                        foreach (var item in Items)
                        {
                            if (item.ToString().Contains(Text))
                            {
                                filterList.Add(item.ToString());
                                if (filterList.Count > FilterMaxCount) break;
                            }
                        }
                    }
                    else
                    {
                        if (dataManager != null)
                        {
                            for (int i = 0; i < Items.Count; i++)
                            {
                                if (GetItemText(dataManager.List[i]).ToString().Contains(Text))
                                {
                                    filterList.Add(dataManager.List[i]);
                                    if (filterList.Count > FilterMaxCount) break;
                                }
                            }
                        }
                    }

                    foreach (var item in filterList)
                    {
                        filterForm.ListBox.Items.Add(GetItemText(item));
                    }
                }
                else
                {
                    filterSelectedItem = null;
                    filterSelectedValue = null;
                }
            }
        }

        List<object> filterList = new List<object>();

        private void ListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!ShowFilter)
                SelectedValueChanged?.Invoke(this, e);
        }

        private void Box_ValueMemberChanged(object sender, EventArgs e)
        {
            ValueMemberChanged?.Invoke(this, e);
        }

        private void Box_DisplayMemberChanged(object sender, EventArgs e)
        {
            DisplayMemberChanged?.Invoke(this, e);
            SetDataConnection();
        }

        private void Box_DataSourceChanged(object sender, EventArgs e)
        {
            DataSourceChanged?.Invoke(this, e);
            SetDataConnection();
        }

        private bool SelectTextChange;

        private void Box_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectTextChange = true;
            if (ListBox.SelectedItem != null && !ShowFilter)
                Text = ListBox.GetItemText(ListBox.SelectedItem);
            SelectTextChange = false;
            SelectedIndexChanged?.Invoke(this, e);
        }

        public event EventHandler SelectedIndexChanged;

        public event EventHandler DataSourceChanged;

        public event EventHandler DisplayMemberChanged;

        public event EventHandler ValueMemberChanged;

        public event EventHandler SelectedValueChanged;

        protected override void ItemForm_ValueChanged(object sender, object value)
        {
            Invalidate();
        }

        private readonly UIComboBoxItem dropForm = new UIComboBoxItem();
        private readonly UIComboBoxItem filterForm = new UIComboBoxItem();

        private UIDropDown filterItemForm;

        private UIDropDown FilterItemForm
        {
            get
            {
                if (filterItemForm == null)
                {
                    filterItemForm = new UIDropDown(filterForm);

                    if (filterItemForm != null)
                    {
                        filterItemForm.VisibleChanged += FilterItemForm_VisibleChanged;
                        filterItemForm.ValueChanged += FilterItemForm_ValueChanged;
                    }
                }

                return filterItemForm;
            }
        }

        private void FilterItemForm_ValueChanged(object sender, object value)
        {
            //
        }

        private void FilterItemForm_VisibleChanged(object sender, EventArgs e)
        {
            dropSymbol = SymbolNormal;
            if (filterItemForm.Visible)
            {
                dropSymbol = SymbolDropDown;
            }

            Invalidate();
        }

        protected override void CreateInstance()
        {
            ItemForm = new UIDropDown(dropForm);
        }

        protected override int CalcItemFormHeight()
        {
            int interval = ItemForm.Height - ItemForm.ClientRectangle.Height;
            return 4 + Math.Min(ListBox.Items.Count, MaxDropDownItems) * ItemHeight + interval;
        }

        private UIListBox ListBox
        {
            get => dropForm.ListBox;
        }

        private UIListBox FilterListBox
        {
            get => dropForm.ListBox;
        }

        [DefaultValue(25)]
        [Description("列表项高度"), Category("SunnyUI")]
        public int ItemHeight
        {
            get => ListBox.ItemHeight;
            set => FilterListBox.ItemHeight = ListBox.ItemHeight = value;
        }

        [DefaultValue(8)]
        [Description("列表下拉最大个数"), Category("SunnyUI")]
        public int MaxDropDownItems { get; set; } = 8;

        private void UIComboBox_FontChanged(object sender, EventArgs e)
        {
            if (ItemForm != null)
            {
                ListBox.Font = Font;
            }

            if (filterForm != null)
            {
                filterForm.ListBox.Font = Font;
            }
        }

        public void ShowDropDown()
        {
            UIComboBox_ButtonClick(this, EventArgs.Empty);
        }

        private void UIComboBox_ButtonClick(object sender, EventArgs e)
        {
            if (!ShowFilter)
            {
                if (Items.Count > 0)
                {
                    ItemForm.Show(this, new Size(DropDownWidth < Width ? Width : DropDownWidth, CalcItemFormHeight()));
                }
            }
            else
            {
                if (FilterItemForm.Visible)
                {
                    FilterItemForm.Close();
                }
                else
                {
                    ShowDropDownFilter();
                }
            }
        }

        public override void SetStyleColor(UIBaseStyle uiColor)
        {
            base.SetStyleColor(uiColor);
            ListBox.SetStyleColor(uiColor.DropDownStyle);
        }

        public object DataSource
        {
            get => ListBox.DataSource;
            set
            {
                ListBox.DataSource = value;
                Box_DataSourceChanged(this, EventArgs.Empty);
            }
        }

        [DefaultValue(150)]
        [Description("下拉框宽度"), Category("SunnyUI")]
        public int DropDownWidth { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Localizable(true)]
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [MergableProperty(false)]
        [Description("列表项"), Category("SunnyUI")]
        public ListBox.ObjectCollection Items => ListBox.Items;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("选中索引"), Category("SunnyUI")]
        public int SelectedIndex
        {
            get => ShowFilter ? -1 : ListBox.SelectedIndex;
            set
            {
                if (!ShowFilter)
                {
                    ListBox.SelectedIndex = value;
                }
            }
        }

        [Browsable(false), Bindable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("选中项"), Category("SunnyUI")]
        public object SelectedItem
        {
            get => ShowFilter ? filterSelectedItem : ListBox.SelectedItem;
            set
            {
                if (!ShowFilter)
                {
                    ListBox.SelectedItem = value;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("选中文字"), Category("SunnyUI")]
        public string SelectedText
        {
            get
            {
                if (DropDownStyle == UIDropDownStyle.DropDown)
                {
                    return edit.SelectedText;
                }
                else
                {
                    return Text;
                }
            }
        }

        public override void ResetText()
        {
            Clear();
        }

        [Description("获取或设置要为此列表框显示的属性。"), Category("SunnyUI")]
        [DefaultValue("")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [TypeConverter("System.Windows.Forms.Design.DataMemberFieldConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string DisplayMember
        {
            get => ListBox.DisplayMember;
            set
            {
                ListBox.DisplayMember = value;
                Box_DisplayMemberChanged(this, EventArgs.Empty);
            }
        }

        [Description("获取或设置指示显示值的方式的格式说明符字符。"), Category("SunnyUI")]
        [DefaultValue("")]
        [Editor("System.Windows.Forms.Design.FormatStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [MergableProperty(false)]
        public string FormatString
        {
            get => ListBox.FormatString;
            set => FilterListBox.FormatString = ListBox.FormatString = value;
        }

        [Description("获取或设置指示显示值是否可以进行格式化操作。"), Category("SunnyUI")]
        [DefaultValue(false)]
        public bool FormattingEnabled
        {
            get => ListBox.FormattingEnabled;
            set => FilterListBox.FormattingEnabled = ListBox.FormattingEnabled = value;
        }

        [Description("获取或设置要为此列表框实际值的属性。"), Category("SunnyUI")]
        [DefaultValue("")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string ValueMember
        {
            get => ListBox.ValueMember;
            set => ListBox.ValueMember = value;
        }

        [
            DefaultValue(null),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            Bindable(true)
        ]
        public object SelectedValue
        {
            get => ShowFilter ? filterSelectedValue : ListBox.SelectedValue;
            set
            {
                if (!ShowFilter)
                    ListBox.SelectedValue = value;
            }
        }

        public string GetItemText(object item)
        {
            return ShowFilter ? FilterListBox.GetItemText(item) : ListBox.GetItemText(item);
        }

        private void UIComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !ShowFilter)
            {
                ShowDropDown();
            }
        }

        [DefaultValue(typeof(Color), "White")]
        public Color ItemFillColor
        {
            get => ListBox.FillColor;
            set => FilterListBox.FillColor = ListBox.FillColor = value;
        }

        [DefaultValue(typeof(Color), "48, 48, 48")]
        public Color ItemForeColor
        {
            get => ListBox.ForeColor;
            set => FilterListBox.ForeColor = ListBox.ForeColor = value;
        }

        [DefaultValue(typeof(Color), "243, 249, 255")]
        public Color ItemSelectForeColor
        {
            get => ListBox.ItemSelectForeColor;
            set => FilterListBox.ItemSelectForeColor = ListBox.ItemSelectForeColor = value;
        }

        [DefaultValue(typeof(Color), "80, 160, 255")]
        public Color ItemSelectBackColor
        {
            get => ListBox.ItemSelectBackColor;
            set => FilterListBox.ItemSelectBackColor = ListBox.ItemSelectBackColor = value;
        }

        [DefaultValue(typeof(Color), "220, 236, 255")]
        public Color ItemHoverColor
        {
            get => ListBox.HoverColor;
            set => FilterListBox.HoverColor = ListBox.HoverColor = value;
        }
    }
}
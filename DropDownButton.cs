using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

#pragma warning disable 1591

namespace UnFound.Controls
{
    public class DropDownButton : Control
    {
        public enum Renderers { Default, Native }

        public delegate void DropDownItemHandler(object sender, DropDownItemEventArgs e);
        /// <summary>
        /// Occurs when the button is clicked.
        /// </summary>
        public new event EventHandler Click;
        /// <summary>
        /// Occurs when the drop down button is clicked, opening the drop down menu.
        /// </summary>
        public event EventHandler DropDownClicked;
        /// <summary>
        /// Occurs when a menu item in the drop down menu is clicked.
        /// </summary>
        public event DropDownItemHandler DropDownItemClicked;

        public DropDownButton()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;
            this.Size = new Size(142, 23);
            this.TextChanged += DropDownButton_TextChanged;
        }

        #region Events

        private void DropDownMenu_ItemAdded(object sender, ToolStripItemEventArgs e)
        {
            if ((sender as DropDownMenu).DropDownButton == this)
                this.Invalidate();
        }

        private void DropDownMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if ((sender as DropDownMenu).DropDownButton == this)
            {
                p_DropDownSelectedItem = p_DropDownMenu.Items.IndexOf(e.ClickedItem);
                (sender as DropDownMenu).Close(ToolStripDropDownCloseReason.ItemClicked);
                this.Invalidate();
                OnDropDownItemClicked(new DropDownItemEventArgs(e.ClickedItem, p_DropDownSelectedItem));
            }
        }

        private void DropDownMenu_ItemRemoved(object sender, ToolStripItemEventArgs e)
        {
            if ((sender as DropDownMenu).DropDownButton == this)
            {
                this.Invalidate();
            }
        }

        private void DropDownButton_TextChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        #endregion

        #region Fields

        private DropDownMenu p_DropDownMenu;
        private int p_DropDownSelectedItem;
        private Renderers p_Renderer = Renderers.Default;

        private Rectangle dropDownRect;
        private int pushedState = 0;
        private PushButtonState buttonState = PushButtonState.Normal;
        private ComboBoxState dropDownState = ComboBoxState.Normal;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the DropDownMenu to display.
        /// </summary>
        [DefaultValue(null)]
        public DropDownMenu DropDownMenu
        {
            get { return p_DropDownMenu; }
            set
            {
                if (p_DropDownMenu != null)
                {
                    p_DropDownMenu.DropDownButton = null;
                    p_DropDownMenu.Renderer = null;
                    p_DropDownMenu.ItemAdded -= DropDownMenu_ItemAdded;
                    p_DropDownMenu.ItemClicked -= DropDownMenu_ItemClicked;
                    p_DropDownMenu.ItemRemoved -= DropDownMenu_ItemRemoved;
                }

                p_DropDownMenu = value;

                if (p_DropDownMenu != null)
                {
                    p_DropDownMenu.DropDownButton = this;
                    p_DropDownMenu.Renderer = p_Renderer.Equals(Renderers.Default) ?
                        null : new NativeToolStripRenderer(new ToolbarTheme());
                    p_DropDownMenu.ItemAdded += DropDownMenu_ItemAdded;
                    p_DropDownMenu.ItemClicked += DropDownMenu_ItemClicked;
                    p_DropDownMenu.ItemRemoved += DropDownMenu_ItemRemoved;
                }

                p_DropDownSelectedItem = 0;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the DropDownMenu selected item. (Last clicked item)
        /// </summary>
        [DefaultValue(0)]
        public int DropDownSelectedItem
        {
            get { return p_DropDownSelectedItem; }
            set
            {
                p_DropDownSelectedItem = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the DropDownMenu renderer.
        /// </summary>
        [DefaultValue(Renderers.Default)]
        public Renderers Renderer
        {
            get { return p_Renderer; }
            set
            {
                if (p_DropDownMenu != null)
                {
                    p_DropDownMenu.Renderer = value == Renderers.Default ?
                        null : new NativeToolStripRenderer(new ToolbarTheme());
                }
                p_Renderer = value;
            }
        }

        #endregion

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData.Equals(Keys.Down))
                return true;
            else
                return base.IsInputKey(keyData);
        }

        protected override void OnClick(EventArgs e)
        {
            // The actual drop down button has it's own event.
            if (dropDownState == ComboBoxState.Pressed)
                return;

            if (this.Click != null)
                this.Click(this, EventArgs.Empty);

            if (p_DropDownMenu == null ||
                p_DropDownMenu.Items.Count == 0 ||
                this.Text != "")
            {
                return;
            }

            if (this.ShowFocusCues)
                this.Focus();

            p_DropDownMenu.DropDownButton = this;
            p_DropDownMenu.Items[p_DropDownSelectedItem].PerformClick();
        }

        protected virtual void OnDropDownClicked(EventArgs e)
        {
            if (DropDownClicked != null)
                DropDownClicked(this, e);
        }

        protected virtual void OnDropDownItemClicked(DropDownItemEventArgs e)
        {
            if (DropDownItemClicked != null)
                DropDownItemClicked(this, e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            this.Invalidate();
            base.OnGotFocus(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
                ShowContextMenuStrip(true);
            else if (e.KeyCode == Keys.Enter)
            {
                if (this.Text == "")
                    OnClick(EventArgs.Empty);
                else
                    ShowContextMenuStrip(true);
            }

            base.OnKeyDown(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            this.Invalidate();
            base.OnLostFocus(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (dropDownRect.Contains(e.Location))
                {
                    buttonState = PushButtonState.Normal;
                    dropDownState = ComboBoxState.Pressed;
                    pushedState = 1;
                }
                else if (this.DisplayRectangle.Contains(e.Location))
                {
                    buttonState = PushButtonState.Pressed;
                    dropDownState = ComboBoxState.Normal;
                    pushedState = 2;
                }

                this.Invalidate();
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            buttonState = PushButtonState.Normal;
            dropDownState = ComboBoxState.Normal;
            this.Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (dropDownRect.Contains(e.Location) && pushedState != 2)
            {
                if (!(buttonState == PushButtonState.Normal &&
                    dropDownState == ComboBoxState.Hot))
                {
                    buttonState = PushButtonState.Normal;
                    dropDownState = ComboBoxState.Hot;
                    this.Invalidate();
                }
            }
            else if (this.DisplayRectangle.Contains(e.Location) && pushedState != 1)
            {
                if (!(buttonState == PushButtonState.Hot &&
                    dropDownState == ComboBoxState.Normal))
                {
                    buttonState = PushButtonState.Hot;
                    dropDownState = ComboBoxState.Normal;
                    this.Invalidate();
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (dropDownRect.Contains(e.Location))
            {
                if (pushedState == 1)
                    ShowContextMenuStrip();

                buttonState = PushButtonState.Normal;
                dropDownState = ComboBoxState.Hot;
            }
            else if (this.DisplayRectangle.Contains(e.Location))
            {
                buttonState = PushButtonState.Hot;
                dropDownState = ComboBoxState.Normal;
            }
            else
            {
                buttonState = PushButtonState.Normal;
                dropDownState = ComboBoxState.Normal;
            }

            pushedState = 0;
            this.Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            string text = string.Empty;
            Rectangle rect = this.DisplayRectangle;
            Rectangle rectText = this.DisplayRectangle;
            StringFormat stringFormat = new StringFormat(); stringFormat.Alignment = StringAlignment.Center; stringFormat.LineAlignment = StringAlignment.Center;

            dropDownRect = new Rectangle(this.DisplayRectangle.Width - 20, this.DisplayRectangle.Y + 1, 20, this.DisplayRectangle.Height - 2);
            rectText.Width -= 20;

            if (this.Text != string.Empty)
            {
                text = this.Text;
                buttonState = PushButtonState.Normal;
            }
            else
            {
                if (!(p_DropDownMenu == null) && p_DropDownMenu.Items.Count > 0)
                    text = p_DropDownMenu.Items[p_DropDownSelectedItem].Text;
                else
                    text = this.Text;
            }

            if (Application.RenderWithVisualStyles)
                DrawVisualStyle(e.Graphics, rect, dropDownRect);
            else
            {
                ButtonState btnState = ButtonState.Normal;
                ButtonState ddbState = ButtonState.Normal;

                switch (buttonState)
                {
                    case PushButtonState.Hot:
                    case PushButtonState.Normal:
                        btnState = ButtonState.Normal;
                        break;
                    case PushButtonState.Pressed:
                        btnState = ButtonState.Pushed;
                        break;
                }

                switch (dropDownState)
                {
                    case ComboBoxState.Hot:
                    case ComboBoxState.Normal:
                        ddbState = ButtonState.Normal;
                        break;
                    case ComboBoxState.Pressed:
                        ddbState = ButtonState.Pushed;
                        break;
                }

                DrawPreVistaStyle(e.Graphics, rect, btnState, dropDownRect, ddbState);
            }

            e.Graphics.DrawString(text, this.Font, new SolidBrush(this.ForeColor), rectText, stringFormat);
            base.OnPaint(e);
        }

        private void DrawVisualStyle(Graphics g, Rectangle rect, Rectangle dropDownRect, bool focused = false)
        {
            bool focus = this.ShowFocusCues && this.Focused;

            if (IsWinXP())
            {
                StringFormat sf = new StringFormat(); sf.Alignment = StringAlignment.Center; sf.LineAlignment = StringAlignment.Center;
                Rectangle rectNew = dropDownRect; rectNew.Y -= 1; rectNew.Height += 2;
                PushButtonState ddState = (PushButtonState)(int)dropDownState;

                ButtonRenderer.DrawButton(g, rect, false, buttonState);
                ButtonRenderer.DrawButton(g, rectNew, focus, ddState | (focus && ddState == PushButtonState.Normal ? PushButtonState.Default : 0));
                g.DrawString("v", this.Font, new SolidBrush(Color.Black), rectNew, sf);
            }
            else
            {
                if (this.Text == "")
                {
                    ButtonRenderer.DrawButton(g, rect, focus, buttonState | (focus && buttonState == PushButtonState.Normal ? PushButtonState.Default : 0));
                    ComboBoxRenderer.DrawDropDownButton(g, dropDownRect, dropDownState);
                }
                else
                {
                    ButtonRenderer.DrawButton(g, rect, false, buttonState);
                    ComboBoxRenderer.DrawDropDownButton(g, dropDownRect, dropDownState | (focus ? ComboBoxState.Hot : 0));

                    if (focus)
                    {
                        Rectangle focusBounds = dropDownRect;
                        focusBounds.Inflate(-2, -2);

                        ControlPaint.DrawBorder(g, focusBounds, Color.Black, ButtonBorderStyle.Dotted);
                    }
                }
            }
        }

        private void DrawPreVistaStyle(Graphics g, Rectangle rect, ButtonState state, Rectangle dropDownRect, ButtonState dropDownState)
        {
            Rectangle newDropDownRect = dropDownRect;
            newDropDownRect.Y -= 2;
            newDropDownRect.Height += 3;
            ControlPaint.DrawButton(g, rect, state);
            ControlPaint.DrawComboButton(g, newDropDownRect, dropDownState);

            if (this.ShowFocusCues && this.Focused)
            {
                Rectangle focusRect = newDropDownRect; focusRect.Inflate(-3, -3);

                newDropDownRect.Y += 1;
                newDropDownRect.Height -= 2;
                focusRect.X -= 1;
                focusRect.Width += 1;
                ControlPaint.DrawBorder(g, focusRect, Color.Black, ButtonBorderStyle.Dotted);
                ControlPaint.DrawBorder(g, newDropDownRect, Color.Black, ButtonBorderStyle.Solid);
            }
        }

        /// <summary>
        /// Shows the DropDownButton at the button.
        /// </summary>
        /// <param name="selectFirstItem">Selects the first item so arrow keys can be used.</param>
        public void ShowContextMenuStrip(bool selectFirstItem = false)
        {
            if (p_DropDownMenu == null)
                return;

            int width = p_DropDownMenu.Width;

            p_DropDownMenu.DropDownButton = this;
            p_DropDownMenu.Show(this, this.DisplayRectangle.Width - width, this.DisplayRectangle.Y + this.Height - 1);

            if (selectFirstItem)
                p_DropDownMenu.Items[0].Select();
        }

        private bool IsWinXP()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT &&
                Environment.OSVersion.Version.Major == 5 &&
                Environment.OSVersion.Version.Minor == 1;
        }
    }

    public class DropDownItemEventArgs : EventArgs
    {
        /// <summary>
        /// Gets clicked ToolStripItem,
        /// </summary>
        public ToolStripItem Item { get; set; }
        /// <summary>
        /// Gets the index of clicked item.
        /// </summary>
        public int ItemIndex { get; set; }

        public DropDownItemEventArgs(ToolStripItem item, int index)
        {
            this.Item = item;
            this.ItemIndex = index;
        }
    }

    public class DropDownMenu : ContextMenuStrip
    {
        /// <summary>
        /// Gets or sets which DropDownButton is currently interacting with the menu.
        /// </summary>
        public DropDownButton DropDownButton { get; set; }
    }
}
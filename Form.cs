#region Assembly System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// C:\Windows\Microsoft.NET\Framework64\v2.0.50727\System.Windows.Forms.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Windows.Forms.Layout;
using System.Windows.Forms.VisualStyles;

namespace System.Windows.Forms
{
  //
  // Summary:
  //     Represents a window or dialog box that makes up an application's user interface.
  [Designer("System.Windows.Forms.Design.FormDocumentDesigner, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",typeof(IRootDesigner))]
  [ToolboxItemFilter("System.Windows.Forms.Control.TopLevel")]
  [DesignerCategory("Form")]
  [DefaultEvent("Load")]
  [ToolboxItem(false)]
  [DesignTimeVisible(false)]
  [ComVisible(true)]
  [InitializationEvent("Load")]
  [ClassInterface(ClassInterfaceType.AutoDispatch)]
  public class Form:ContainerControl
  {
    //
    // Summary:
    //     Represents a collection of controls on the form.
    [ComVisible(false)]
    public new class ControlCollection:Control.ControlCollection
    {
      private Form owner;

      //
      // Summary:
      //     Initializes a new instance of the System.Windows.Forms.Form.ControlCollection
      //     class.
      //
      // Parameters:
      //   owner:
      //     The System.Windows.Forms.Form to contain the controls added to the control collection.
      public ControlCollection(Form owner)
        : base(owner)
      {
        this.owner=owner;
      }

      //
      // Summary:
      //     Adds a control to the form.
      //
      // Parameters:
      //   value:
      //     The System.Windows.Forms.Control to add to the form.
      //
      // Exceptions:
      //   T:System.Exception:
      //     A multiple document interface (MDI) parent form cannot have controls added to
      //     it.
      public override void Add(Control value)
      {
        if(value is MdiClient&&owner.ctlClient==null)
        {
          if(!owner.TopLevel&&!owner.DesignMode)
          {
            throw new ArgumentException(SR.GetString("MDIContainerMustBeTopLevel"),"value");
          }

          owner.AutoScroll=false;
          if(owner.IsMdiChild)
          {
            throw new ArgumentException(SR.GetString("FormMDIParentAndChild"),"value");
          }

          owner.ctlClient=(MdiClient)value;
        }

        if(value is Form&&((Form)value).MdiParentInternal!=null)
        {
          throw new ArgumentException(SR.GetString("FormMDIParentCannotAdd"),"value");
        }

        base.Add(value);
        if(owner.ctlClient!=null)
        {
          owner.ctlClient.SendToBack();
        }
      }

      //
      // Summary:
      //     Removes a control from the form.
      //
      // Parameters:
      //   value:
      //     A System.Windows.Forms.Control to remove from the form.
      public override void Remove(Control value)
      {
        if(value==owner.ctlClient)
        {
          owner.ctlClient=null;
        }

        base.Remove(value);
      }
    }

    private class EnumThreadWindowsCallback
    {
      private List<HandleRef> ownedWindows;

      internal EnumThreadWindowsCallback()
      {
      }

      internal bool Callback(IntPtr hWnd,IntPtr lParam)
      {
        HandleRef handleRef = new HandleRef(null,hWnd);
        IntPtr windowLong = UnsafeNativeMethods.GetWindowLong(handleRef,-8);
        if(windowLong==lParam)
        {
          if(ownedWindows==null)
          {
            ownedWindows=new List<HandleRef>();
          }

          ownedWindows.Add(handleRef);
        }

        return true;
      }

      internal void ResetOwners()
      {
        if(ownedWindows==null)
        {
          return;
        }

        foreach(HandleRef ownedWindow in ownedWindows)
        {
          UnsafeNativeMethods.SetWindowLong(ownedWindow,-8,NativeMethods.NullHandleRef);
        }
      }

      internal void SetOwners(HandleRef hRefOwner)
      {
        if(ownedWindows==null)
        {
          return;
        }

        foreach(HandleRef ownedWindow in ownedWindows)
        {
          UnsafeNativeMethods.SetWindowLong(ownedWindow,-8,hRefOwner);
        }
      }
    }

    private class SecurityToolTip:IDisposable
    {
      private sealed class ToolTipNativeWindow:NativeWindow
      {
        private SecurityToolTip control;

        internal ToolTipNativeWindow(SecurityToolTip control)
        {
          this.control=control;
        }

        protected override void WndProc(ref Message m)
        {
          if(control!=null)
          {
            control.WndProc(ref m);
          }
        }
      }

      private Form owner;

      private string toolTipText;

      private bool first = true;

      private ToolTipNativeWindow window;

      private CreateParams CreateParams
      {
        get
        {
          NativeMethods.INITCOMMONCONTROLSEX iNITCOMMONCONTROLSEX = new NativeMethods.INITCOMMONCONTROLSEX();
          iNITCOMMONCONTROLSEX.dwICC=8;
          SafeNativeMethods.InitCommonControlsEx(iNITCOMMONCONTROLSEX);
          CreateParams createParams = new CreateParams();
          createParams.Parent=owner.Handle;
          createParams.ClassName="tooltips_class32";
          createParams.Style|=65;
          createParams.ExStyle=0;
          createParams.Caption=null;
          return createParams;
        }
      }

      internal bool Modal => first;

      internal SecurityToolTip(Form owner)
      {
        this.owner=owner;
        SetupText();
        window=new ToolTipNativeWindow(this);
        SetupToolTip();
        owner.LocationChanged+=FormLocationChanged;
        owner.HandleCreated+=FormHandleCreated;
      }

      public void Dispose()
      {
        if(owner!=null)
        {
          owner.LocationChanged-=FormLocationChanged;
        }

        if(window.Handle!=IntPtr.Zero)
        {
          window.DestroyHandle();
          window=null;
        }
      }

      private NativeMethods.TOOLINFO_T GetTOOLINFO()
      {
        NativeMethods.TOOLINFO_T tOOLINFO_T = new NativeMethods.TOOLINFO_T();
        tOOLINFO_T.cbSize=Marshal.SizeOf(typeof(NativeMethods.TOOLINFO_T));
        tOOLINFO_T.uFlags|=16;
        tOOLINFO_T.lpszText=toolTipText;
        if(owner.RightToLeft==RightToLeft.Yes)
        {
          tOOLINFO_T.uFlags|=4;
        }

        if(!first)
        {
          tOOLINFO_T.uFlags|=256;
          tOOLINFO_T.hwnd=owner.Handle;
          Rectangle r = new Rectangle(width: SystemInformation.CaptionButtonSize.Width,x: owner.Left,y: owner.Top,height: SystemInformation.CaptionHeight);
          r=owner.RectangleToClient(r);
          r.Width-=r.X;
          r.Y++;
          tOOLINFO_T.rect=NativeMethods.RECT.FromXYWH(r.X,r.Y,r.Width,r.Height);
          tOOLINFO_T.uId=IntPtr.Zero;
        }
        else
        {
          tOOLINFO_T.uFlags|=33;
          tOOLINFO_T.hwnd=IntPtr.Zero;
          tOOLINFO_T.uId=owner.Handle;
        }

        return tOOLINFO_T;
      }

      private void SetupText()
      {
        owner.EnsureSecurityInformation();
        string @string = SR.GetString("SecurityToolTipMainText");
        string string2 = SR.GetString("SecurityToolTipSourceInformation",owner.securitySite);
        toolTipText=SR.GetString("SecurityToolTipTextFormat",@string,string2);
      }

      private void SetupToolTip()
      {
        window.CreateHandle(CreateParams);
        SafeNativeMethods.SetWindowPos(new HandleRef(window,window.Handle),NativeMethods.HWND_TOPMOST,0,0,0,0,19);
        UnsafeNativeMethods.SendMessage(new HandleRef(window,window.Handle),1048,0,owner.Width);
        UnsafeNativeMethods.SendMessage(new HandleRef(window,window.Handle),NativeMethods.TTM_SETTITLE,2,SR.GetString("SecurityToolTipCaption"));
        _=(int)UnsafeNativeMethods.SendMessage(new HandleRef(window,window.Handle),NativeMethods.TTM_ADDTOOL,0,GetTOOLINFO());
        UnsafeNativeMethods.SendMessage(new HandleRef(window,window.Handle),1025,1,0);
        Show();
      }

      private void RecreateHandle()
      {
        if(window!=null)
        {
          if(window.Handle!=IntPtr.Zero)
          {
            window.DestroyHandle();
          }

          SetupToolTip();
        }
      }

      private void FormHandleCreated(object sender,EventArgs e)
      {
        RecreateHandle();
      }

      private void FormLocationChanged(object sender,EventArgs e)
      {
        if(window!=null&&first)
        {
          Size captionButtonSize = SystemInformation.CaptionButtonSize;
          if(owner.WindowState==FormWindowState.Minimized)
          {
            Pop(noLongerFirst: true);
          }
          else
          {
            UnsafeNativeMethods.SendMessage(new HandleRef(window,window.Handle),1042,0,NativeMethods.Util.MAKELONG(owner.Left+captionButtonSize.Width/2,owner.Top+SystemInformation.CaptionHeight));
          }
        }
        else
        {
          Pop(noLongerFirst: true);
        }
      }

      internal void Pop(bool noLongerFirst)
      {
        if(noLongerFirst)
        {
          first=false;
        }

        UnsafeNativeMethods.SendMessage(new HandleRef(window,window.Handle),1041,0,GetTOOLINFO());
        UnsafeNativeMethods.SendMessage(new HandleRef(window,window.Handle),NativeMethods.TTM_DELTOOL,0,GetTOOLINFO());
        UnsafeNativeMethods.SendMessage(new HandleRef(window,window.Handle),NativeMethods.TTM_ADDTOOL,0,GetTOOLINFO());
      }

      internal void Show()
      {
        if(first)
        {
          Size captionButtonSize = SystemInformation.CaptionButtonSize;
          UnsafeNativeMethods.SendMessage(new HandleRef(window,window.Handle),1042,0,NativeMethods.Util.MAKELONG(owner.Left+captionButtonSize.Width/2,owner.Top+SystemInformation.CaptionHeight));
          UnsafeNativeMethods.SendMessage(new HandleRef(window,window.Handle),1041,1,GetTOOLINFO());
        }
      }

      private void WndProc(ref Message msg)
      {
        if(first&&(msg.Msg==513||msg.Msg==516||msg.Msg==519||msg.Msg==523))
        {
          Pop(noLongerFirst: true);
        }

        window.DefWndProc(ref msg);
      }
    }

    private const int SizeGripSize = 16;

    private static readonly object EVENT_ACTIVATED = new object();

    private static readonly object EVENT_CLOSING = new object();

    private static readonly object EVENT_CLOSED = new object();

    private static readonly object EVENT_FORMCLOSING = new object();

    private static readonly object EVENT_FORMCLOSED = new object();

    private static readonly object EVENT_DEACTIVATE = new object();

    private static readonly object EVENT_LOAD = new object();

    private static readonly object EVENT_MDI_CHILD_ACTIVATE = new object();

    private static readonly object EVENT_INPUTLANGCHANGE = new object();

    private static readonly object EVENT_INPUTLANGCHANGEREQUEST = new object();

    private static readonly object EVENT_MENUSTART = new object();

    private static readonly object EVENT_MENUCOMPLETE = new object();

    private static readonly object EVENT_MAXIMUMSIZECHANGED = new object();

    private static readonly object EVENT_MINIMUMSIZECHANGED = new object();

    private static readonly object EVENT_HELPBUTTONCLICKED = new object();

    private static readonly object EVENT_SHOWN = new object();

    private static readonly object EVENT_RESIZEBEGIN = new object();

    private static readonly object EVENT_RESIZEEND = new object();

    private static readonly object EVENT_RIGHTTOLEFTLAYOUTCHANGED = new object();

    private static readonly BitVector32.Section FormStateAllowTransparency = BitVector32.CreateSection(1);

    private static readonly BitVector32.Section FormStateBorderStyle = BitVector32.CreateSection(6,FormStateAllowTransparency);

    private static readonly BitVector32.Section FormStateTaskBar = BitVector32.CreateSection(1,FormStateBorderStyle);

    private static readonly BitVector32.Section FormStateControlBox = BitVector32.CreateSection(1,FormStateTaskBar);

    private static readonly BitVector32.Section FormStateKeyPreview = BitVector32.CreateSection(1,FormStateControlBox);

    private static readonly BitVector32.Section FormStateLayered = BitVector32.CreateSection(1,FormStateKeyPreview);

    private static readonly BitVector32.Section FormStateMaximizeBox = BitVector32.CreateSection(1,FormStateLayered);

    private static readonly BitVector32.Section FormStateMinimizeBox = BitVector32.CreateSection(1,FormStateMaximizeBox);

    private static readonly BitVector32.Section FormStateHelpButton = BitVector32.CreateSection(1,FormStateMinimizeBox);

    private static readonly BitVector32.Section FormStateStartPos = BitVector32.CreateSection(4,FormStateHelpButton);

    private static readonly BitVector32.Section FormStateWindowState = BitVector32.CreateSection(2,FormStateStartPos);

    private static readonly BitVector32.Section FormStateShowWindowOnCreate = BitVector32.CreateSection(1,FormStateWindowState);

    private static readonly BitVector32.Section FormStateAutoScaling = BitVector32.CreateSection(1,FormStateShowWindowOnCreate);

    private static readonly BitVector32.Section FormStateSetClientSize = BitVector32.CreateSection(1,FormStateAutoScaling);

    private static readonly BitVector32.Section FormStateTopMost = BitVector32.CreateSection(1,FormStateSetClientSize);

    private static readonly BitVector32.Section FormStateSWCalled = BitVector32.CreateSection(1,FormStateTopMost);

    private static readonly BitVector32.Section FormStateMdiChildMax = BitVector32.CreateSection(1,FormStateSWCalled);

    private static readonly BitVector32.Section FormStateRenderSizeGrip = BitVector32.CreateSection(1,FormStateMdiChildMax);

    private static readonly BitVector32.Section FormStateSizeGripStyle = BitVector32.CreateSection(2,FormStateRenderSizeGrip);

    private static readonly BitVector32.Section FormStateIsRestrictedWindow = BitVector32.CreateSection(1,FormStateSizeGripStyle);

    private static readonly BitVector32.Section FormStateIsRestrictedWindowChecked = BitVector32.CreateSection(1,FormStateIsRestrictedWindow);

    private static readonly BitVector32.Section FormStateIsWindowActivated = BitVector32.CreateSection(1,FormStateIsRestrictedWindowChecked);

    private static readonly BitVector32.Section FormStateIsTextEmpty = BitVector32.CreateSection(1,FormStateIsWindowActivated);

    private static readonly BitVector32.Section FormStateIsActive = BitVector32.CreateSection(1,FormStateIsTextEmpty);

    private static readonly BitVector32.Section FormStateIconSet = BitVector32.CreateSection(1,FormStateIsActive);

    private static readonly BitVector32.Section FormStateExCalledClosing = BitVector32.CreateSection(1);

    private static readonly BitVector32.Section FormStateExUpdateMenuHandlesSuspendCount = BitVector32.CreateSection(8,FormStateExCalledClosing);

    private static readonly BitVector32.Section FormStateExUpdateMenuHandlesDeferred = BitVector32.CreateSection(1,FormStateExUpdateMenuHandlesSuspendCount);

    private static readonly BitVector32.Section FormStateExUseMdiChildProc = BitVector32.CreateSection(1,FormStateExUpdateMenuHandlesDeferred);

    private static readonly BitVector32.Section FormStateExCalledOnLoad = BitVector32.CreateSection(1,FormStateExUseMdiChildProc);

    private static readonly BitVector32.Section FormStateExCalledMakeVisible = BitVector32.CreateSection(1,FormStateExCalledOnLoad);

    private static readonly BitVector32.Section FormStateExCalledCreateControl = BitVector32.CreateSection(1,FormStateExCalledMakeVisible);

    private static readonly BitVector32.Section FormStateExAutoSize = BitVector32.CreateSection(1,FormStateExCalledCreateControl);

    private static readonly BitVector32.Section FormStateExInUpdateMdiControlStrip = BitVector32.CreateSection(1,FormStateExAutoSize);

    private static readonly BitVector32.Section FormStateExShowIcon = BitVector32.CreateSection(1,FormStateExInUpdateMdiControlStrip);

    private static readonly BitVector32.Section FormStateExMnemonicProcessed = BitVector32.CreateSection(1,FormStateExShowIcon);

    private static readonly BitVector32.Section FormStateExInScale = BitVector32.CreateSection(1,FormStateExMnemonicProcessed);

    private static readonly BitVector32.Section FormStateExInModalSizingLoop = BitVector32.CreateSection(1,FormStateExInScale);

    private static readonly BitVector32.Section FormStateExSettingAutoScale = BitVector32.CreateSection(1,FormStateExInModalSizingLoop);

    private static readonly BitVector32.Section FormStateExWindowBoundsWidthIsClientSize = BitVector32.CreateSection(1,FormStateExSettingAutoScale);

    private static readonly BitVector32.Section FormStateExWindowBoundsHeightIsClientSize = BitVector32.CreateSection(1,FormStateExWindowBoundsWidthIsClientSize);

    private static readonly BitVector32.Section FormStateExWindowClosing = BitVector32.CreateSection(1,FormStateExWindowBoundsHeightIsClientSize);

    private static Icon defaultIcon = null;

    private static Icon defaultRestrictedIcon = null;

    private static Padding FormPadding = new Padding(9);

    private static object internalSyncObject = new object();

    private static readonly int PropAcceptButton = PropertyStore.CreateKey();

    private static readonly int PropCancelButton = PropertyStore.CreateKey();

    private static readonly int PropDefaultButton = PropertyStore.CreateKey();

    private static readonly int PropDialogOwner = PropertyStore.CreateKey();

    private static readonly int PropMainMenu = PropertyStore.CreateKey();

    private static readonly int PropDummyMenu = PropertyStore.CreateKey();

    private static readonly int PropCurMenu = PropertyStore.CreateKey();

    private static readonly int PropMergedMenu = PropertyStore.CreateKey();

    private static readonly int PropOwner = PropertyStore.CreateKey();

    private static readonly int PropOwnedForms = PropertyStore.CreateKey();

    private static readonly int PropMaximizedBounds = PropertyStore.CreateKey();

    private static readonly int PropOwnedFormsCount = PropertyStore.CreateKey();

    private static readonly int PropMinTrackSizeWidth = PropertyStore.CreateKey();

    private static readonly int PropMinTrackSizeHeight = PropertyStore.CreateKey();

    private static readonly int PropMaxTrackSizeWidth = PropertyStore.CreateKey();

    private static readonly int PropMaxTrackSizeHeight = PropertyStore.CreateKey();

    private static readonly int PropFormMdiParent = PropertyStore.CreateKey();

    private static readonly int PropActiveMdiChild = PropertyStore.CreateKey();

    private static readonly int PropFormerlyActiveMdiChild = PropertyStore.CreateKey();

    private static readonly int PropMdiChildFocusable = PropertyStore.CreateKey();

    private static readonly int PropMainMenuStrip = PropertyStore.CreateKey();

    private static readonly int PropMdiWindowListStrip = PropertyStore.CreateKey();

    private static readonly int PropMdiControlStrip = PropertyStore.CreateKey();

    private static readonly int PropSecurityTip = PropertyStore.CreateKey();

    private static readonly int PropOpacity = PropertyStore.CreateKey();

    private static readonly int PropTransparencyKey = PropertyStore.CreateKey();

    private BitVector32 formState = new BitVector32(135992);

    private BitVector32 formStateEx = default(BitVector32);

    private Icon icon;

    private Icon smallIcon;

    private Size autoScaleBaseSize = Size.Empty;

    private Size minAutoSize = Size.Empty;

    private Rectangle restoredWindowBounds = new Rectangle(-1,-1,-1,-1);

    private BoundsSpecified restoredWindowBoundsSpecified;

    private DialogResult dialogResult;

    private MdiClient ctlClient;

    private NativeWindow ownerWindow;

    private string userWindowText;

    private string securityZone;

    private string securitySite;

    private bool rightToLeftLayout;

    private Rectangle restoreBounds = new Rectangle(-1,-1,-1,-1);

    private CloseReason closeReason;

    private VisualStyleRenderer sizeGripRenderer;

    private static readonly object EVENT_MAXIMIZEDBOUNDSCHANGED = new object();

    //
    // Summary:
    //     Gets or sets the button on the form that is clicked when the user presses the
    //     ENTER key.
    //
    // Returns:
    //     An System.Windows.Forms.IButtonControl that represents the button to use as the
    //     accept button for the form.
    [SRDescription("FormAcceptButtonDescr")]
    [DefaultValue(null)]
    public IButtonControl AcceptButton
    {
      get
      {
        return (IButtonControl)base.Properties.GetObject(PropAcceptButton);
      }
      set
      {
        if(AcceptButton!=value)
        {
          base.Properties.SetObject(PropAcceptButton,value);
          UpdateDefaultButton();
        }
      }
    }

    internal bool Active
    {
      get
      {
        Form parentFormInternal = base.ParentFormInternal;
        if(parentFormInternal==null)
        {
          return formState[FormStateIsActive]!=0;
        }

        if(parentFormInternal.ActiveControl==this)
        {
          return parentFormInternal.Active;
        }

        return false;
      }
      set
      {
        if(formState[FormStateIsActive]!=0==value||(value&&!CanRecreateHandle()))
        {
          return;
        }

        formState[FormStateIsActive]=(value ? 1 : 0);
        if(value)
        {
          formState[FormStateIsWindowActivated]=1;
          if(IsRestrictedWindow)
          {
            WindowText=userWindowText;
          }

          if(!base.ValidationCancelled)
          {
            if(base.ActiveControl==null)
            {
              SelectNextControlInternal(null,forward: true,tabStopOnly: true,nested: true,wrap: false);
            }

            base.InnerMostActiveContainerControl.FocusActiveControlInternal();
          }

          OnActivated(EventArgs.Empty);
        }
        else
        {
          formState[FormStateIsWindowActivated]=0;
          if(IsRestrictedWindow)
          {
            Text=userWindowText;
          }

          OnDeactivate(EventArgs.Empty);
        }
      }
    }

    //
    // Summary:
    //     Gets the currently active form for this application.
    //
    // Returns:
    //     A System.Windows.Forms.Form that represents the currently active form, or null
    //     if there is no active form.
    public static Form ActiveForm
    {
      get
      {
        IntSecurity.GetParent.Demand();
        IntPtr foregroundWindow = UnsafeNativeMethods.GetForegroundWindow();
        Control control = Control.FromHandleInternal(foregroundWindow);
        if(control!=null&&control is Form)
        {
          return (Form)control;
        }

        return null;
      }
    }

    //
    // Summary:
    //     Gets the currently active multiple-document interface (MDI) child window.
    //
    // Returns:
    //     Returns a System.Windows.Forms.Form that represents the currently active MDI
    //     child window, or null if there are currently no child windows present.
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    [SRDescription("FormActiveMDIChildDescr")]
    public Form ActiveMdiChild
    {
      get
      {
        Form form = ActiveMdiChildInternal;
        if(form==null&&ctlClient!=null&&ctlClient.IsHandleCreated)
        {
          IntPtr handle = ctlClient.SendMessage(553,0,0);
          form=Control.FromHandleInternal(handle) as Form;
        }

        if(form!=null&&form.Visible&&form.Enabled)
        {
          return form;
        }

        return null;
      }
    }

    internal Form ActiveMdiChildInternal
    {
      get
      {
        return (Form)base.Properties.GetObject(PropActiveMdiChild);
      }
      set
      {
        base.Properties.SetObject(PropActiveMdiChild,value);
      }
    }

    private Form FormerlyActiveMdiChild
    {
      get
      {
        return (Form)base.Properties.GetObject(PropFormerlyActiveMdiChild);
      }
      set
      {
        base.Properties.SetObject(PropFormerlyActiveMdiChild,value);
      }
    }

    //
    // Summary:
    //     Gets or sets a value indicating whether the opacity of the form can be adjusted.
    //
    // Returns:
    //     true if the opacity of the form can be changed; otherwise, false.
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [SRDescription("ControlAllowTransparencyDescr")]
    public bool AllowTransparency
    {
      get
      {
        return formState[FormStateAllowTransparency]!=0;
      }
      set
      {
        if(value==(formState[FormStateAllowTransparency]!=0)||!OSFeature.Feature.IsPresent(OSFeature.LayeredWindows))
        {
          return;
        }

        formState[FormStateAllowTransparency]=(value ? 1 : 0);
        formState[FormStateLayered]=formState[FormStateAllowTransparency];
        UpdateStyles();
        if(!value)
        {
          if(base.Properties.ContainsObject(PropOpacity))
          {
            base.Properties.SetObject(PropOpacity,1f);
          }

          if(base.Properties.ContainsObject(PropTransparencyKey))
          {
            base.Properties.SetObject(PropTransparencyKey,Color.Empty);
          }

          UpdateLayered();
        }
      }
    }

    //
    // Summary:
    //     Gets or sets a value indicating whether the form adjusts its size to fit the
    //     height of the font used on the form and scales its controls.
    //
    // Returns:
    //     true if the form will automatically scale itself and its controls based on the
    //     current font assigned to the form; otherwise, false. The default is true.
    [SRCategory("CatLayout")]
    [Obsolete("This property has been deprecated. Use the AutoScaleMode property instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
    [SRDescription("FormAutoScaleDescr")]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool AutoScale
    {
      get
      {
        return formState[FormStateAutoScaling]!=0;
      }
      set
      {
        formStateEx[FormStateExSettingAutoScale]=1;
        try
        {
          if(value)
          {
            formState[FormStateAutoScaling]=1;
            base.AutoScaleMode=AutoScaleMode.None;
          }
          else
          {
            formState[FormStateAutoScaling]=0;
          }
        }
        finally
        {
          formStateEx[FormStateExSettingAutoScale]=0;
        }
      }
    }

    //
    // Summary:
    //     Gets or sets the base size used for autoscaling of the form.
    //
    // Returns:
    //     A System.Drawing.Size that represents the base size that this form uses for autoscaling.
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Localizable(true)]
    public virtual Size AutoScaleBaseSize
    {
      get
      {
        if(autoScaleBaseSize.IsEmpty)
        {
          SizeF autoScaleSize = GetAutoScaleSize(Font);
          return new Size((int)Math.Round(autoScaleSize.Width),(int)Math.Round(autoScaleSize.Height));
        }

        return autoScaleBaseSize;
      }
      set
      {
        autoScaleBaseSize=value;
      }
    }

    //
    // Summary:
    //     Gets or sets a value indicating whether the form enables autoscrolling.
    //
    // Returns:
    //     true to enable autoscrolling on the form; otherwise, false. The default is false.
    [Localizable(true)]
    public override bool AutoScroll
    {
      get
      {
        return base.AutoScroll;
      }
      set
      {
        if(value)
        {
          IsMdiContainer=false;
        }

        base.AutoScroll=value;
      }
    }

    //
    // Summary:
    //     Resize the form according to the setting of System.Windows.Forms.Form.AutoSizeMode.
    //
    // Returns:
    //     true if the form will automatically resize; false if it must be manually resized.
    [EditorBrowsable(EditorBrowsableState.Always)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [Browsable(true)]
    public override bool AutoSize
    {
      get
      {
        return formStateEx[FormStateExAutoSize]!=0;
      }
      set
      {
        if(value!=AutoSize)
        {
          formStateEx[FormStateExAutoSize]=(value ? 1 : 0);
          if(!AutoSize)
          {
            minAutoSize=Size.Empty;
            Size=CommonProperties.GetSpecifiedBounds(this).Size;
          }

          LayoutTransaction.DoLayout(this,this,PropertyNames.AutoSize);
          OnAutoSizeChanged(EventArgs.Empty);
        }
      }
    }

    //
    // Summary:
    //     Gets or sets the mode by which the form automatically resizes itself.
    //
    // Returns:
    //     An System.Windows.Forms.AutoSizeMode enumerated value. The default is System.Windows.Forms.AutoSizeMode.GrowOnly.
    //
    // Exceptions:
    //   T:System.ComponentModel.InvalidEnumArgumentException:
    //     The value is not a valid System.Windows.Forms.AutoSizeMode value.
    [SRDescription("ControlAutoSizeModeDescr")]
    [Localizable(true)]
    [SRCategory("CatLayout")]
    [Browsable(true)]
    [DefaultValue(AutoSizeMode.GrowOnly)]
    public AutoSizeMode AutoSizeMode
    {
      get
      {
        return GetAutoSizeMode();
      }
      set
      {
        if(!ClientUtils.IsEnumValid(value,(int)value,0,1))
        {
          throw new InvalidEnumArgumentException("value",(int)value,typeof(AutoSizeMode));
        }

        if(GetAutoSizeMode()==value)
        {
          return;
        }

        SetAutoSizeMode(value);
        Control control = ((base.DesignMode||ParentInternal==null) ? this : ParentInternal);
        if(control!=null)
        {
          if(control.LayoutEngine==DefaultLayout.Instance)
          {
            control.LayoutEngine.InitLayout(this,BoundsSpecified.Size);
          }

          LayoutTransaction.DoLayout(control,this,PropertyNames.AutoSize);
        }
      }
    }

    //
    // Returns:
    //     An System.Windows.Forms.AutoValidate enumerated value that indicates whether
    //     contained controls are implicitly validated on focus change. The default is System.Windows.Forms.AutoValidate.Inherit.
    [Browsable(true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public override AutoValidate AutoValidate
    {
      get
      {
        return base.AutoValidate;
      }
      set
      {
        base.AutoValidate=value;
      }
    }

    //
    // Returns:
    //     A System.Drawing.Color that represents the background color of the control. The
    //     default is the value of the System.Windows.Forms.Control.DefaultBackColor property.
    public override Color BackColor
    {
      get
      {
        Color rawBackColor = base.RawBackColor;
        if(!rawBackColor.IsEmpty)
        {
          return rawBackColor;
        }

        return Control.DefaultBackColor;
      }
      set
      {
        base.BackColor=value;
      }
    }

    private bool CalledClosing
    {
      get
      {
        return formStateEx[FormStateExCalledClosing]!=0;
      }
      set
      {
        formStateEx[FormStateExCalledClosing]=(value ? 1 : 0);
      }
    }

    private bool CalledCreateControl
    {
      get
      {
        return formStateEx[FormStateExCalledCreateControl]!=0;
      }
      set
      {
        formStateEx[FormStateExCalledCreateControl]=(value ? 1 : 0);
      }
    }

    private bool CalledMakeVisible
    {
      get
      {
        return formStateEx[FormStateExCalledMakeVisible]!=0;
      }
      set
      {
        formStateEx[FormStateExCalledMakeVisible]=(value ? 1 : 0);
      }
    }

    private bool CalledOnLoad
    {
      get
      {
        return formStateEx[FormStateExCalledOnLoad]!=0;
      }
      set
      {
        formStateEx[FormStateExCalledOnLoad]=(value ? 1 : 0);
      }
    }

    //
    // Summary:
    //     Gets or sets the border style of the form.
    //
    // Returns:
    //     A System.Windows.Forms.FormBorderStyle that represents the style of border to
    //     display for the form. The default is FormBorderStyle.Sizable.
    //
    // Exceptions:
    //   T:System.ComponentModel.InvalidEnumArgumentException:
    //     The value specified is outside the range of valid values.
    [DefaultValue(FormBorderStyle.Sizable)]
    [DispId(-504)]
    [SRDescription("FormBorderStyleDescr")]
    [SRCategory("CatAppearance")]
    public FormBorderStyle FormBorderStyle
    {
      get
      {
        return (FormBorderStyle)formState[FormStateBorderStyle];
      }
      set
      {
        if(!ClientUtils.IsEnumValid(value,(int)value,0,6))
        {
          throw new InvalidEnumArgumentException("value",(int)value,typeof(FormBorderStyle));
        }

        if(IsRestrictedWindow)
        {
          switch(value)
          {
            case FormBorderStyle.None:
              value=FormBorderStyle.FixedSingle;
              break;
            case FormBorderStyle.FixedToolWindow:
              value=FormBorderStyle.FixedSingle;
              break;
            case FormBorderStyle.SizableToolWindow:
              value=FormBorderStyle.Sizable;
              break;
            default:
              value=FormBorderStyle.Sizable;
              break;
            case FormBorderStyle.FixedSingle:
            case FormBorderStyle.Fixed3D:
            case FormBorderStyle.FixedDialog:
            case FormBorderStyle.Sizable:
              break;
          }
        }

        formState[FormStateBorderStyle]=(int)value;
        if(formState[FormStateSetClientSize]==1&&!base.IsHandleCreated)
        {
          ClientSize=ClientSize;
        }

        Rectangle rectangle = restoredWindowBounds;
        BoundsSpecified boundsSpecified = restoredWindowBoundsSpecified;
        int value2 = formStateEx[FormStateExWindowBoundsWidthIsClientSize];
        int value3 = formStateEx[FormStateExWindowBoundsHeightIsClientSize];
        UpdateFormStyles();
        if(formState[FormStateIconSet]==0&&!IsRestrictedWindow)
        {
          UpdateWindowIcon(redrawFrame: false);
        }

        if(WindowState!=0)
        {
          restoredWindowBounds=rectangle;
          restoredWindowBoundsSpecified=boundsSpecified;
          formStateEx[FormStateExWindowBoundsWidthIsClientSize]=value2;
          formStateEx[FormStateExWindowBoundsHeightIsClientSize]=value3;
        }
      }
    }

    //
    // Summary:
    //     Gets or sets the button control that is clicked when the user presses the ESC
    //     key.
    //
    // Returns:
    //     An System.Windows.Forms.IButtonControl that represents the cancel button for
    //     the form.
    [DefaultValue(null)]
    [SRDescription("FormCancelButtonDescr")]
    public IButtonControl CancelButton
    {
      get
      {
        return (IButtonControl)base.Properties.GetObject(PropCancelButton);
      }
      set
      {
        base.Properties.SetObject(PropCancelButton,value);
        if(value!=null&&value.DialogResult==DialogResult.None)
        {
          value.DialogResult=DialogResult.Cancel;
        }
      }
    }

    //
    // Summary:
    //     Gets or sets the size of the client area of the form.
    //
    // Returns:
    //     A System.Drawing.Size that represents the size of the form's client area.
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [Localizable(true)]
    public new Size ClientSize
    {
      get
      {
        return base.ClientSize;
      }
      set
      {
        base.ClientSize=value;
      }
    }

    //
    // Summary:
    //     Gets or sets a value indicating whether a control box is displayed in the caption
    //     bar of the form.
    //
    // Returns:
    //     true if the form displays a control box in the upper left corner of the form;
    //     otherwise, false. The default is true.
    [SRDescription("FormControlBoxDescr")]
    [SRCategory("CatWindowStyle")]
    [DefaultValue(true)]
    public bool ControlBox
    {
      get
      {
        return formState[FormStateControlBox]!=0;
      }
      set
      {
        if(!IsRestrictedWindow)
        {
          if(value)
          {
            formState[FormStateControlBox]=1;
          }
          else
          {
            formState[FormStateControlBox]=0;
          }

          UpdateFormStyles();
        }
      }
    }

    protected override CreateParams CreateParams
    {
      [SecurityPermission(SecurityAction.LinkDemand,Flags = SecurityPermissionFlag.UnmanagedCode)]
      get
      {
        CreateParams createParams = base.CreateParams;
        if(base.IsHandleCreated&&((uint)base.WindowStyle&0x8000000u)!=0)
        {
          createParams.Style|=134217728;
        }
        else if(TopLevel)
        {
          createParams.Style&=-134217729;
        }

        if(TopLevel&&formState[FormStateLayered]!=0)
        {
          createParams.ExStyle|=524288;
        }

        IWin32Window win32Window = (IWin32Window)base.Properties.GetObject(PropDialogOwner);
        if(win32Window!=null)
        {
          createParams.Parent=Control.GetSafeHandle(win32Window);
        }

        FillInCreateParamsBorderStyles(createParams);
        FillInCreateParamsWindowState(createParams);
        FillInCreateParamsBorderIcons(createParams);
        if(formState[FormStateTaskBar]!=0)
        {
          createParams.ExStyle|=262144;
        }

        FormBorderStyle formBorderStyle = FormBorderStyle;
        if(!ShowIcon&&(formBorderStyle==FormBorderStyle.Sizable||formBorderStyle==FormBorderStyle.Fixed3D||formBorderStyle==FormBorderStyle.FixedSingle))
        {
          createParams.ExStyle|=1;
        }

        if(IsMdiChild)
        {
          if(base.Visible&&(WindowState==FormWindowState.Maximized||WindowState==FormWindowState.Normal))
          {
            Form form = (Form)base.Properties.GetObject(PropFormMdiParent);
            Form activeMdiChildInternal = form.ActiveMdiChildInternal;
            if(activeMdiChildInternal!=null&&activeMdiChildInternal.WindowState==FormWindowState.Maximized)
            {
              createParams.Style|=16777216;
              formState[FormStateWindowState]=2;
              SetState(65536,value: true);
            }
          }

          if(formState[FormStateMdiChildMax]!=0)
          {
            createParams.Style|=16777216;
          }

          createParams.ExStyle|=64;
        }

        if(TopLevel||IsMdiChild)
        {
          FillInCreateParamsStartPosition(createParams);
          if(((uint)createParams.Style&0x10000000u)!=0)
          {
            formState[FormStateShowWindowOnCreate]=1;
            createParams.Style&=-268435457;
          }
          else
          {
            formState[FormStateShowWindowOnCreate]=0;
          }
        }

        if(IsRestrictedWindow)
        {
          createParams.Caption=RestrictedWindowText(createParams.Caption);
        }

        if(RightToLeft==RightToLeft.Yes&&RightToLeftLayout)
        {
          createParams.ExStyle|=5242880;
          createParams.ExStyle&=-28673;
        }

        return createParams;
      }
    }

    internal CloseReason CloseReason
    {
      get
      {
        return closeReason;
      }
      set
      {
        closeReason=value;
      }
    }

    internal static Icon DefaultIcon
    {
      get
      {
        if(defaultIcon==null)
        {
          lock(internalSyncObject)
          {
            if(defaultIcon==null)
            {
              defaultIcon=new Icon(typeof(Form),"wfc.ico");
            }
          }
        }

        return defaultIcon;
      }
    }

    //
    // Summary:
    //     Gets the default Input Method Editor (IME) mode supported by the control.
    //
    // Returns:
    //     One of the System.Windows.Forms.ImeMode values.
    protected override ImeMode DefaultImeMode => ImeMode.NoControl;

    private static Icon DefaultRestrictedIcon
    {
      get
      {
        if(defaultRestrictedIcon==null)
        {
          lock(internalSyncObject)
          {
            if(defaultRestrictedIcon==null)
            {
              defaultRestrictedIcon=new Icon(typeof(Form),"wfsecurity.ico");
            }
          }
        }

        return defaultRestrictedIcon;
      }
    }

    //
    // Returns:
    //     The default System.Drawing.Size of the control.
    protected override Size DefaultSize => new Size(300,300);

    //
    // Summary:
    //     Gets or sets the size and location of the form on the Windows desktop.
    //
    // Returns:
    //     A System.Drawing.Rectangle that represents the bounds of the form on the Windows
    //     desktop using desktop coordinates.
    [SRDescription("FormDesktopBoundsDescr")]
    [Browsable(false)]
    [SRCategory("CatLayout")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Rectangle DesktopBounds
    {
      get
      {
        Rectangle workingArea = SystemInformation.WorkingArea;
        Rectangle bounds = base.Bounds;
        bounds.X-=workingArea.X;
        bounds.Y-=workingArea.Y;
        return bounds;
      }
      set
      {
        SetDesktopBounds(value.X,value.Y,value.Width,value.Height);
      }
    }

    //
    // Summary:
    //     Gets or sets the location of the form on the Windows desktop.
    //
    // Returns:
    //     A System.Drawing.Point that represents the location of the form on the desktop.
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [SRDescription("FormDesktopLocationDescr")]
    [SRCategory("CatLayout")]
    [Browsable(false)]
    public Point DesktopLocation
    {
      get
      {
        Rectangle workingArea = SystemInformation.WorkingArea;
        Point location = Location;
        location.X-=workingArea.X;
        location.Y-=workingArea.Y;
        return location;
      }
      set
      {
        SetDesktopLocation(value.X,value.Y);
      }
    }

    //
    // Summary:
    //     Gets or sets the dialog result for the form.
    //
    // Returns:
    //     A System.Windows.Forms.DialogResult that represents the result of the form when
    //     used as a dialog box.
    //
    // Exceptions:
    //   T:System.ComponentModel.InvalidEnumArgumentException:
    //     The value specified is outside the range of valid values.
    [SRCategory("CatBehavior")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [SRDescription("FormDialogResultDescr")]
    [Browsable(false)]
    public DialogResult DialogResult
    {
      get
      {
        return dialogResult;
      }
      set
      {
        if(!ClientUtils.IsEnumValid(value,(int)value,0,7))
        {
          throw new InvalidEnumArgumentException("value",(int)value,typeof(DialogResult));
        }

        dialogResult=value;
      }
    }

    internal override bool HasMenu
    {
      get
      {
        bool result = false;
        Menu menu = Menu;
        if(TopLevel&&menu!=null&&menu.ItemCount>0)
        {
          result=true;
        }

        return result;
      }
    }

    //
    // Summary:
    //     Gets or sets a value indicating whether a Help button should be displayed in
    //     the caption box of the form.
    //
    // Returns:
    //     true to display a Help button in the form's caption bar; otherwise, false. The
    //     default is false.
    [DefaultValue(false)]
    [SRDescription("FormHelpButtonDescr")]
    [SRCategory("CatWindowStyle")]
    public bool HelpButton
    {
      get
      {
        return formState[FormStateHelpButton]!=0;
      }
      set
      {
        if(value)
        {
          formState[FormStateHelpButton]=1;
        }
        else
        {
          formState[FormStateHelpButton]=0;
        }

        UpdateFormStyles();
      }
    }

    //
    // Summary:
    //     Gets or sets the icon for the form.
    //
    // Returns:
    //     An System.Drawing.Icon that represents the icon for the form.
    [Localizable(true)]
    [AmbientValue(null)]
    [SRCategory("CatWindowStyle")]
    [SRDescription("FormIconDescr")]
    public Icon Icon
    {
      get
      {
        if(formState[FormStateIconSet]==0)
        {
          if(IsRestrictedWindow)
          {
            return DefaultRestrictedIcon;
          }

          return DefaultIcon;
        }

        return icon;
      }
      set
      {
        if(icon!=value&&!IsRestrictedWindow)
        {
          if(value==defaultIcon)
          {
            value=null;
          }

          formState[FormStateIconSet]=((value!=null) ? 1 : 0);
          icon=value;
          if(smallIcon!=null)
          {
            smallIcon.Dispose();
            smallIcon=null;
          }

          UpdateWindowIcon(redrawFrame: true);
        }
      }
    }

    private bool IsClosing
    {
      get
      {
        return formStateEx[FormStateExWindowClosing]==1;
      }
      set
      {
        formStateEx[FormStateExWindowClosing]=(value ? 1 : 0);
      }
    }

    private bool IsMaximized
    {
      get
      {
        if(WindowState!=FormWindowState.Maximized)
        {
          if(IsMdiChild)
          {
            return formState[FormStateMdiChildMax]==1;
          }

          return false;
        }

        return true;
      }
    }

    //
    // Summary:
    //     Gets a value indicating whether the form is a multiple-document interface (MDI)
    //     child form.
    //
    // Returns:
    //     true if the form is an MDI child form; otherwise, false.
    [SRCategory("CatWindowStyle")]
    [SRDescription("FormIsMDIChildDescr")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public bool IsMdiChild => base.Properties.GetObject(PropFormMdiParent)!=null;

    internal bool IsMdiChildFocusable
    {
      get
      {
        if(base.Properties.ContainsObject(PropMdiChildFocusable))
        {
          return (bool)base.Properties.GetObject(PropMdiChildFocusable);
        }

        return false;
      }
      set
      {
        if(value!=IsMdiChildFocusable)
        {
          base.Properties.SetObject(PropMdiChildFocusable,value);
        }
      }
    }

    //
    // Summary:
    //     Gets or sets a value indicating whether the form is a container for multiple-document
    //     interface (MDI) child forms.
    //
    // Returns:
    //     true if the form is a container for MDI child forms; otherwise, false. The default
    //     is false.
    [SRCategory("CatWindowStyle")]
    [SRDescription("FormIsMDIContainerDescr")]
    [DefaultValue(false)]
    public bool IsMdiContainer
    {
      get
      {
        return ctlClient!=null;
      }
      set
      {
        if(value!=IsMdiContainer)
        {
          if(value)
          {
            AllowTransparency=false;
            base.Controls.Add(new MdiClient());
          }
          else
          {
            ActiveMdiChildInternal=null;
            ctlClient.Dispose();
          }

          Invalidate();
        }
      }
    }

    //
    // Summary:
    //     Gets a value indicating whether the form can use all windows and user input events
    //     without restriction.
    //
    // Returns:
    //     true if the form has restrictions; otherwise, false. The default is true.
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public bool IsRestrictedWindow
    {
      get
      {
        if(formState[FormStateIsRestrictedWindowChecked]==0)
        {
          formState[FormStateIsRestrictedWindow]=0;
          try
          {
            IntSecurity.WindowAdornmentModification.Demand();
          }
          catch(SecurityException)
          {
            formState[FormStateIsRestrictedWindow]=1;
          }
          catch
          {
            formState[FormStateIsRestrictedWindow]=1;
            formState[FormStateIsRestrictedWindowChecked]=1;
            throw;
          }

          formState[FormStateIsRestrictedWindowChecked]=1;
        }

        return formState[FormStateIsRestrictedWindow]!=0;
      }
    }

    //
    // Summary:
    //     Gets or sets a value indicating whether the form will receive key events before
    //     the event is passed to the control that has focus.
    //
    // Returns:
    //     true if the form will receive all key events; false if the currently selected
    //     control on the form receives key events. The default is false.
    [DefaultValue(false)]
    [SRDescription("FormKeyPreviewDescr")]
    public bool KeyPreview
    {
      get
      {
        return formState[FormStateKeyPreview]!=0;
      }
      set
      {
        if(value)
        {
          formState[FormStateKeyPreview]=1;
        }
        else
        {
          formState[FormStateKeyPreview]=0;
        }
      }
    }

    //
    // Summary:
    //     Gets or sets the System.Drawing.Point that represents the upper-left corner of
    //     the System.Windows.Forms.Form in screen coordinates.
    //
    // Returns:
    //     The System.Drawing.Point that represents the upper-left corner of the System.Windows.Forms.Form
    //     in screen coordinates.
    [SettingsBindable(true)]
    public new Point Location
    {
      get
      {
        return base.Location;
      }
      set
      {
        base.Location=value;
      }
    }

    //
    // Summary:
    //     Gets and sets the size of the form when it is maximized.
    //
    // Returns:
    //     A System.Drawing.Rectangle that represents the bounds of the form when it is
    //     maximized.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     The value of the System.Drawing.Rectangle.Top property is greater than the height
    //     of the form.-or- The value of the System.Drawing.Rectangle.Left property is greater
    //     than the width of the form.
    protected Rectangle MaximizedBounds
    {
      get
      {
        return base.Properties.GetRectangle(PropMaximizedBounds);
      }
      set
      {
        if(!value.Equals(MaximizedBounds))
        {
          base.Properties.SetRectangle(PropMaximizedBounds,value);
          OnMaximizedBoundsChanged(EventArgs.Empty);
        }
      }
    }

    //
    // Summary:
    //     Gets the maximum size the form can be resized to.
    //
    // Returns:
    //     A System.Drawing.Size that represents the maximum size for the form.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     The values of the height or width within the System.Drawing.Size object are less
    //     than zero.
    [DefaultValue(typeof(Size),"0, 0")]
    [Localizable(true)]
    [RefreshProperties(RefreshProperties.Repaint)]
    [SRCategory("CatLayout")]
    [SRDescription("FormMaximumSizeDescr")]
    public override Size MaximumSize
    {
      get
      {
        if(base.Properties.ContainsInteger(PropMaxTrackSizeWidth))
        {
          return new Size(base.Properties.GetInteger(PropMaxTrackSizeWidth),base.Properties.GetInteger(PropMaxTrackSizeHeight));
        }

        return Size.Empty;
      }
      set
      {
        if(value.Equals(MaximumSize))
        {
          return;
        }

        if(value.Width<0||value.Height<0)
        {
          throw new ArgumentOutOfRangeException("MaximumSize");
        }

        base.Properties.SetInteger(PropMaxTrackSizeWidth,value.Width);
        base.Properties.SetInteger(PropMaxTrackSizeHeight,value.Height);
        if(!MinimumSize.IsEmpty&&!value.IsEmpty)
        {
          if(base.Properties.GetInteger(PropMinTrackSizeWidth)>value.Width)
          {
            base.Properties.SetInteger(PropMinTrackSizeWidth,value.Width);
          }

          if(base.Properties.GetInteger(PropMinTrackSizeHeight)>value.Height)
          {
            base.Properties.SetInteger(PropMinTrackSizeHeight,value.Height);
          }
        }

        Size size = Size;
        if(!value.IsEmpty&&(size.Width>value.Width||size.Height>value.Height))
        {
          Size=new Size(Math.Min(size.Width,value.Width),Math.Min(size.Height,value.Height));
        }

        OnMaximumSizeChanged(EventArgs.Empty);
      }
    }

    //
    // Summary:
    //     Gets or sets the primary menu container for the form.
    //
    // Returns:
    //     A System.Windows.Forms.MenuStrip that represents the container for the menu structure
    //     of the form. The default is null.
    [SRDescription("FormMenuStripDescr")]
    [TypeConverter(typeof(ReferenceConverter))]
    [SRCategory("CatWindowStyle")]
    [DefaultValue(null)]
    public MenuStrip MainMenuStrip
    {
      get
      {
        return (MenuStrip)base.Properties.GetObject(PropMainMenuStrip);
      }
      set
      {
        base.Properties.SetObject(PropMainMenuStrip,value);
        if(base.IsHandleCreated&&Menu==null)
        {
          UpdateMenuHandles();
        }
      }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new Padding Margin
    {
      get
      {
        return base.Margin;
      }
      set
      {
        base.Margin=value;
      }
    }

    //
    // Summary:
    //     Gets or sets the System.Windows.Forms.MainMenu that is displayed in the form.
    //
    // Returns:
    //     A System.Windows.Forms.MainMenu that represents the menu to display in the form.
    [SRDescription("FormMenuDescr")]
    [Browsable(false)]
    [DefaultValue(null)]
    [SRCategory("CatWindowStyle")]
    [TypeConverter(typeof(ReferenceConverter))]
    public MainMenu Menu
    {
      get
      {
        return (MainMenu)base.Properties.GetObject(PropMainMenu);
      }
      set
      {
        MainMenu menu = Menu;
        if(menu==value)
        {
          return;
        }

        if(menu!=null)
        {
          menu.form=null;
        }

        base.Properties.SetObject(PropMainMenu,value);
        if(value!=null)
        {
          if(value.form!=null)
          {
            value.form.Menu=null;
          }

          value.form=this;
        }

        if(formState[FormStateSetClientSize]==1&&!base.IsHandleCreated)
        {
          ClientSize=ClientSize;
        }

        MenuChanged(0,value);
      }
    }

    //
    // Summary:
    //     Gets or sets the minimum size the form can be resized to.
    //
    // Returns:
    //     A System.Drawing.Size that represents the minimum size for the form.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     The values of the height or width within the System.Drawing.Size object are less
    //     than zero.
    [SRCategory("CatLayout")]
    [SRDescription("FormMinimumSizeDescr")]
    [Localizable(true)]
    [RefreshProperties(RefreshProperties.Repaint)]
    public override Size MinimumSize
    {
      get
      {
        if(base.Properties.ContainsInteger(PropMinTrackSizeWidth))
        {
          return new Size(base.Properties.GetInteger(PropMinTrackSizeWidth),base.Properties.GetInteger(PropMinTrackSizeHeight));
        }

        return DefaultMinimumSize;
      }
      set
      {
        if(value.Equals(MinimumSize))
        {
          return;
        }

        if(value.Width<0||value.Height<0)
        {
          throw new ArgumentOutOfRangeException("MinimumSize");
        }

        Rectangle bounds = base.Bounds;
        bounds.Size=value;
        value=WindowsFormsUtils.ConstrainToScreenWorkingAreaBounds(bounds).Size;
        base.Properties.SetInteger(PropMinTrackSizeWidth,value.Width);
        base.Properties.SetInteger(PropMinTrackSizeHeight,value.Height);
        if(!MaximumSize.IsEmpty&&!value.IsEmpty)
        {
          if(base.Properties.GetInteger(PropMaxTrackSizeWidth)<value.Width)
          {
            base.Properties.SetInteger(PropMaxTrackSizeWidth,value.Width);
          }

          if(base.Properties.GetInteger(PropMaxTrackSizeHeight)<value.Height)
          {
            base.Properties.SetInteger(PropMaxTrackSizeHeight,value.Height);
          }
        }

        Size size = Size;
        if(size.Width<value.Width||size.Height<value.Height)
        {
          Size=new Size(Math.Max(size.Width,value.Width),Math.Max(size.Height,value.Height));
        }

        if(base.IsHandleCreated)
        {
          SafeNativeMethods.SetWindowPos(new HandleRef(this,base.Handle),NativeMethods.NullHandleRef,Location.X,Location.Y,Size.Width,Size.Height,4);
        }

        OnMinimumSizeChanged(EventArgs.Empty);
      }
    }

    //
    // Summary:
    //     Gets or sets a value indicating whether the Maximize button is displayed in the
    //     caption bar of the form.
    //
    // Returns:
    //     true to display a Maximize button for the form; otherwise, false. The default
    //     is true.
    [SRCategory("CatWindowStyle")]
    [DefaultValue(true)]
    [SRDescription("FormMaximizeBoxDescr")]
    public bool MaximizeBox
    {
      get
      {
        return formState[FormStateMaximizeBox]!=0;
      }
      set
      {
        if(value)
        {
          formState[FormStateMaximizeBox]=1;
        }
        else
        {
          formState[FormStateMaximizeBox]=0;
        }

        UpdateFormStyles();
      }
    }

    //
    // Summary:
    //     Gets an array of forms that represent the multiple-document interface (MDI) child
    //     forms that are parented to this form.
    //
    // Returns:
    //     An array of System.Windows.Forms.Form objects, each of which identifies one of
    //     this form's MDI child forms.
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [SRDescription("FormMDIChildrenDescr")]
    [SRCategory("CatWindowStyle")]
    public Form[] MdiChildren
    {
      get
      {
        if(ctlClient!=null)
        {
          return ctlClient.MdiChildren;
        }

        return new Form[0];
      }
    }

    internal MdiClient MdiClient => ctlClient;

    //
    // Summary:
    //     Gets or sets the current multiple-document interface (MDI) parent form of this
    //     form.
    //
    // Returns:
    //     A System.Windows.Forms.Form that represents the MDI parent form.
    //
    // Exceptions:
    //   T:System.Exception:
    //     The System.Windows.Forms.Form assigned to this property is not marked as an MDI
    //     container.-or- The System.Windows.Forms.Form assigned to this property is both
    //     a child and an MDI container form.-or- The System.Windows.Forms.Form assigned
    //     to this property is located on a different thread.
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [SRDescription("FormMDIParentDescr")]
    [Browsable(false)]
    [SRCategory("CatWindowStyle")]
    public Form MdiParent
    {
      get
      {
        IntSecurity.GetParent.Demand();
        return MdiParentInternal;
      }
      set
      {
        MdiParentInternal=value;
      }
    }

    private Form MdiParentInternal
    {
      get
      {
        return (Form)base.Properties.GetObject(PropFormMdiParent);
      }
      set
      {
        Form form = (Form)base.Properties.GetObject(PropFormMdiParent);
        if(value==form&&(value!=null||ParentInternal==null))
        {
          return;
        }

        if(value!=null&&base.CreateThreadId!=value.CreateThreadId)
        {
          throw new ArgumentException(SR.GetString("AddDifferentThreads"),"value");
        }

        bool visible = GetState(2);
        base.Visible=false;
        try
        {
          if(value==null)
          {
            ParentInternal=null;
            SetTopLevel(value: true);
          }
          else
          {
            if(IsMdiContainer)
            {
              throw new ArgumentException(SR.GetString("FormMDIParentAndChild"),"value");
            }

            if(!value.IsMdiContainer)
            {
              throw new ArgumentException(SR.GetString("MDIParentNotContainer"),"value");
            }

            Dock=DockStyle.None;
            base.Properties.SetObject(PropFormMdiParent,value);
            SetState(524288,value: false);
            ParentInternal=value.MdiClient;
            if(ParentInternal.IsHandleCreated&&IsMdiChild&&base.IsHandleCreated)
            {
              DestroyHandle();
            }
          }

          InvalidateMergedMenu();
          UpdateMenuHandles();
        }
        finally
        {
          UpdateStyles();
          base.Visible=visible;
        }
      }
    }

    private MdiWindowListStrip MdiWindowListStrip
    {
      get
      {
        return base.Properties.GetObject(PropMdiWindowListStrip) as MdiWindowListStrip;
      }
      set
      {
        base.Properties.SetObject(PropMdiWindowListStrip,value);
      }
    }

    private MdiControlStrip MdiControlStrip
    {
      get
      {
        return base.Properties.GetObject(PropMdiControlStrip) as MdiControlStrip;
      }
      set
      {
        base.Properties.SetObject(PropMdiControlStrip,value);
      }
    }

    //
    // Summary:
    //     Gets the merged menu for the form.
    //
    // Returns:
    //     A System.Windows.Forms.MainMenu that represents the merged menu of the form.
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [SRDescription("FormMergedMenuDescr")]
    [SRCategory("CatWindowStyle")]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public MainMenu MergedMenu
    {
      [UIPermission(SecurityAction.Demand,Window = UIPermissionWindow.AllWindows)]
      get
      {
        return MergedMenuPrivate;
      }
    }

    private MainMenu MergedMenuPrivate
    {
      get
      {
        Form form = (Form)base.Properties.GetObject(PropFormMdiParent);
        if(form==null)
        {
          return null;
        }

        MainMenu mainMenu = (MainMenu)base.Properties.GetObject(PropMergedMenu);
        if(mainMenu!=null)
        {
          return mainMenu;
        }

        MainMenu menu = form.Menu;
        MainMenu menu2 = Menu;
        if(menu2==null)
        {
          return menu;
        }

        if(menu==null)
        {
          return menu2;
        }

        mainMenu=new MainMenu();
        mainMenu.ownerForm=this;
        mainMenu.MergeMenu(menu);
        mainMenu.MergeMenu(menu2);
        base.Properties.SetObject(PropMergedMenu,mainMenu);
        return mainMenu;
      }
    }

    //
    // Summary:
    //     Gets or sets a value indicating whether the Minimize button is displayed in the
    //     caption bar of the form.
    //
    // Returns:
    //     true to display a Minimize button for the form; otherwise, false. The default
    //     is true.
    [SRCategory("CatWindowStyle")]
    [DefaultValue(true)]
    [SRDescription("FormMinimizeBoxDescr")]
    public bool MinimizeBox
    {
      get
      {
        return formState[FormStateMinimizeBox]!=0;
      }
      set
      {
        if(value)
        {
          formState[FormStateMinimizeBox]=1;
        }
        else
        {
          formState[FormStateMinimizeBox]=0;
        }

        UpdateFormStyles();
      }
    }

    //
    // Summary:
    //     Gets a value indicating whether this form is displayed modally.
    //
    // Returns:
    //     true if the form is displayed modally; otherwise, false.
    [SRDescription("FormModalDescr")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [SRCategory("CatWindowStyle")]
    [Browsable(false)]
    public bool Modal => GetState(32);

    //
    // Summary:
    //     Gets or sets the opacity level of the form.
    //
    // Returns:
    //     The level of opacity for the form. The default is 1.00.
    [DefaultValue(1.0)]
    [TypeConverter(typeof(OpacityConverter))]
    [SRDescription("FormOpacityDescr")]
    [SRCategory("CatWindowStyle")]
    public double Opacity
    {
      get
      {
        object @object = base.Properties.GetObject(PropOpacity);
        if(@object!=null)
        {
          return Convert.ToDouble(@object,CultureInfo.InvariantCulture);
        }

        return 1.0;
      }
      set
      {
        if(IsRestrictedWindow)
        {
          value=Math.Max(value,0.5);
        }

        if(value>1.0)
        {
          value=1.0;
        }
        else if(value<0.0)
        {
          value=0.0;
        }

        base.Properties.SetObject(PropOpacity,value);
        bool flag = formState[FormStateLayered]!=0;
        if(OpacityAsByte<byte.MaxValue&&OSFeature.Feature.IsPresent(OSFeature.LayeredWindows))
        {
          AllowTransparency=true;
          if(formState[FormStateLayered]!=1)
          {
            formState[FormStateLayered]=1;
            if(!flag)
            {
              UpdateStyles();
            }
          }
        }
        else
        {
          formState[FormStateLayered]=((TransparencyKey!=Color.Empty) ? 1 : 0);
          if(flag!=(formState[FormStateLayered]!=0))
          {
            int num = (int)(long)UnsafeNativeMethods.GetWindowLong(new HandleRef(this,base.Handle),-20);
            CreateParams createParams = CreateParams;
            if(num!=createParams.ExStyle)
            {
              UnsafeNativeMethods.SetWindowLong(new HandleRef(this,base.Handle),-20,new HandleRef(null,(IntPtr)createParams.ExStyle));
            }
          }
        }

        UpdateLayered();
      }
    }

    private byte OpacityAsByte => (byte)(Opacity*255.0);

    //
    // Summary:
    //     Gets an array of System.Windows.Forms.Form objects that represent all forms that
    //     are owned by this form.
    //
    // Returns:
    //     A System.Windows.Forms.Form array that represents the owned forms for this form.
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    [SRDescription("FormOwnedFormsDescr")]
    [SRCategory("CatWindowStyle")]
    public Form[] OwnedForms
    {
      get
      {
        Form[] sourceArray = (Form[])base.Properties.GetObject(PropOwnedForms);
        int integer = base.Properties.GetInteger(PropOwnedFormsCount);
        Form[] array = new Form[integer];
        if(integer>0)
        {
          Array.Copy(sourceArray,0,array,0,integer);
        }

        return array;
      }
    }

    //
    // Summary:
    //     Gets or sets the form that owns this form.
    //
    // Returns:
    //     A System.Windows.Forms.Form that represents the form that is the owner of this
    //     form.
    //
    // Exceptions:
    //   T:System.Exception:
    //     A top-level window cannot have an owner.
    [Browsable(false)]
    [SRCategory("CatWindowStyle")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [SRDescription("FormOwnerDescr")]
    public Form Owner
    {
      get
      {
        IntSecurity.GetParent.Demand();
        return OwnerInternal;
      }
      set
      {
        Form ownerInternal = OwnerInternal;
        if(ownerInternal!=value)
        {
          if(value!=null&&!TopLevel)
          {
            throw new ArgumentException(SR.GetString("NonTopLevelCantHaveOwner"),"value");
          }

          Control.CheckParentingCycle(this,value);
          Control.CheckParentingCycle(value,this);
          base.Properties.SetObject(PropOwner,null);
          ownerInternal?.RemoveOwnedForm(this);
          base.Properties.SetObject(PropOwner,value);
          value?.AddOwnedForm(this);
          UpdateHandleWithOwner();
        }
      }
    }

    internal Form OwnerInternal => (Form)base.Properties.GetObject(PropOwner);

    //
    // Summary:
    //     Gets the location and size of the form in its normal window state.
    //
    // Returns:
    //     A System.Drawing.Rectangle that contains the location and size of the form in
    //     the normal window state.
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public Rectangle RestoreBounds
    {
      get
      {
        if(restoreBounds.Width==-1&&restoreBounds.Height==-1&&restoreBounds.X==-1&&restoreBounds.Y==-1)
        {
          return base.Bounds;
        }

        return restoreBounds;
      }
    }

    //
    // Summary:
    //     Gets or sets a value indicating whether right-to-left mirror placement is turned
    //     on.
    //
    // Returns:
    //     true if right-to-left mirror placement is turned on; otherwise, false for standard
    //     child control placement. The default is false.
    [SRDescription("ControlRightToLeftLayoutDescr")]
    [Localizable(true)]
    [DefaultValue(false)]
    [SRCategory("CatAppearance")]
    public virtual bool RightToLeftLayout
    {
      get
      {
        return rightToLeftLayout;
      }
      set
      {
        if(value!=rightToLeftLayout)
        {
          rightToLeftLayout=value;
          using(new LayoutTransaction(this,this,PropertyNames.RightToLeftLayout))
          {
            OnRightToLeftLayoutChanged(EventArgs.Empty);
          }
        }
      }
    }

    internal override Control ParentInternal
    {
      get
      {
        return base.ParentInternal;
      }
      set
      {
        if(value!=null)
        {
          Owner=null;
        }

        base.ParentInternal=value;
      }
    }

    //
    // Summary:
    //     Gets or sets a value indicating whether the form is displayed in the Windows
    //     taskbar.
    //
    // Returns:
    //     true to display the form in the Windows taskbar at run time; otherwise, false.
    //     The default is true.
    [DefaultValue(true)]
    [SRCategory("CatWindowStyle")]
    [SRDescription("FormShowInTaskbarDescr")]
    public bool ShowInTaskbar
    {
      get
      {
        return formState[FormStateTaskBar]!=0;
      }
      set
      {
        if(!IsRestrictedWindow&&ShowInTaskbar!=value)
        {
          if(value)
          {
            formState[FormStateTaskBar]=1;
          }
          else
          {
            formState[FormStateTaskBar]=0;
          }

          if(base.IsHandleCreated)
          {
            RecreateHandle();
          }
        }
      }
    }

    //
    // Summary:
    //     Gets or sets a value indicating whether an icon is displayed in the caption bar
    //     of the form.
    //
    // Returns:
    //     true if the form displays an icon in the caption bar; otherwise, false. The default
    //     is true.
    [SRCategory("CatWindowStyle")]
    [SRDescription("FormShowIconDescr")]
    [DefaultValue(true)]
    public bool ShowIcon
    {
      get
      {
        return formStateEx[FormStateExShowIcon]!=0;
      }
      set
      {
        if(value)
        {
          formStateEx[FormStateExShowIcon]=1;
        }
        else
        {
          if(IsRestrictedWindow)
          {
            return;
          }

          formStateEx[FormStateExShowIcon]=0;
          UpdateStyles();
        }

        UpdateWindowIcon(redrawFrame: true);
      }
    }

    internal override int ShowParams
    {
      get
      {
        switch(WindowState)
        {
          case FormWindowState.Maximized:
            return 3;
          case FormWindowState.Minimized:
            return 2;
          default:
            if(ShowWithoutActivation)
            {
              return 4;
            }

            return 5;
        }
      }
    }

    //
    // Summary:
    //     Gets a value indicating whether the window will be activated when it is shown.
    //
    // Returns:
    //     True if the window will not be activated when it is shown; otherwise, false.
    //     The default is false.
    [Browsable(false)]
    protected virtual bool ShowWithoutActivation => false;

    //
    // Summary:
    //     Gets or sets the size of the form.
    //
    // Returns:
    //     A System.Drawing.Size that represents the size of the form.
    [Localizable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Size Size
    {
      get
      {
        return base.Size;
      }
      set
      {
        base.Size=value;
      }
    }

    //
    // Summary:
    //     Gets or sets the style of the size grip to display in the lower-right corner
    //     of the form.
    //
    // Returns:
    //     A System.Windows.Forms.SizeGripStyle that represents the style of the size grip
    //     to display. The default is System.Windows.Forms.SizeGripStyle.Auto
    //
    // Exceptions:
    //   T:System.ComponentModel.InvalidEnumArgumentException:
    //     The value specified is outside the range of valid values.
    [DefaultValue(SizeGripStyle.Auto)]
    [SRDescription("FormSizeGripStyleDescr")]
    [SRCategory("CatWindowStyle")]
    public SizeGripStyle SizeGripStyle
    {
      get
      {
        return (SizeGripStyle)formState[FormStateSizeGripStyle];
      }
      set
      {
        if(SizeGripStyle!=value)
        {
          if(!ClientUtils.IsEnumValid(value,(int)value,0,2))
          {
            throw new InvalidEnumArgumentException("value",(int)value,typeof(SizeGripStyle));
          }

          formState[FormStateSizeGripStyle]=(int)value;
          UpdateRenderSizeGrip();
        }
      }
    }

    //
    // Summary:
    //     Gets or sets the starting position of the form at run time.
    //
    // Returns:
    //     A System.Windows.Forms.FormStartPosition that represents the starting position
    //     of the form.
    //
    // Exceptions:
    //   T:System.ComponentModel.InvalidEnumArgumentException:
    //     The value specified is outside the range of valid values.
    [SRCategory("CatLayout")]
    [SRDescription("FormStartPositionDescr")]
    [Localizable(true)]
    [DefaultValue(FormStartPosition.WindowsDefaultLocation)]
    public FormStartPosition StartPosition
    {
      get
      {
        return (FormStartPosition)formState[FormStateStartPos];
      }
      set
      {
        if(!ClientUtils.IsEnumValid(value,(int)value,0,4))
        {
          throw new InvalidEnumArgumentException("value",(int)value,typeof(FormStartPosition));
        }

        formState[FormStateStartPos]=(int)value;
      }
    }

    //
    // Summary:
    //     Gets or sets the tab order of the control within its container.
    //
    // Returns:
    //     An System.Int32 containing the index of the control within the set of controls
    //     within its container that is included in the tab order.
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new int TabIndex
    {
      get
      {
        return base.TabIndex;
      }
      set
      {
        base.TabIndex=value;
      }
    }

    [DispId(-516)]
    [SRDescription("ControlTabStopDescr")]
    [Browsable(false)]
    [DefaultValue(true)]
    [SRCategory("CatBehavior")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new bool TabStop
    {
      get
      {
        return base.TabStop;
      }
      set
      {
        base.TabStop=value;
      }
    }

    private HandleRef TaskbarOwner
    {
      get
      {
        if(ownerWindow==null)
        {
          ownerWindow=new NativeWindow();
        }

        if(ownerWindow.Handle==IntPtr.Zero)
        {
          CreateParams createParams = new CreateParams();
          createParams.ExStyle=128;
          ownerWindow.CreateHandle(createParams);
        }

        return new HandleRef(ownerWindow,ownerWindow.Handle);
      }
    }

    //
    // Returns:
    //     The text associated with this control.
    [SettingsBindable(true)]
    public override string Text
    {
      get
      {
        return base.Text;
      }
      set
      {
        base.Text=value;
      }
    }

    //
    // Summary:
    //     Gets or sets a value indicating whether to display the form as a top-level window.
    //
    // Returns:
    //     true to display the form as a top-level window; otherwise, false. The default
    //     is true.
    //
    // Exceptions:
    //   T:System.Exception:
    //     A Multiple-document interface (MDI) parent form must be a top-level window.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public bool TopLevel
    {
      get
      {
        return GetTopLevel();
      }
      set
      {
        if(!value&&IsMdiContainer&&!base.DesignMode)
        {
          throw new ArgumentException(SR.GetString("MDIContainerMustBeTopLevel"),"value");
        }

        SetTopLevel(value);
      }
    }

    //
    // Summary:
    //     Gets or sets a value indicating whether the form should be displayed as a topmost
    //     form.
    //
    // Returns:
    //     true to display the form as a topmost form; otherwise, false. The default is
    //     false.
    [SRCategory("CatWindowStyle")]
    [DefaultValue(false)]
    [SRDescription("FormTopMostDescr")]
    public bool TopMost
    {
      get
      {
        return formState[FormStateTopMost]!=0;
      }
      set
      {
        if(!IsRestrictedWindow)
        {
          if(base.IsHandleCreated&&TopLevel)
          {
            HandleRef hWndInsertAfter = (value ? NativeMethods.HWND_TOPMOST : NativeMethods.HWND_NOTOPMOST);
            SafeNativeMethods.SetWindowPos(new HandleRef(this,base.Handle),hWndInsertAfter,0,0,0,0,3);
          }

          if(value)
          {
            formState[FormStateTopMost]=1;
          }
          else
          {
            formState[FormStateTopMost]=0;
          }
        }
      }
    }

    //
    // Summary:
    //     Gets or sets the color that will represent transparent areas of the form.
    //
    // Returns:
    //     A System.Drawing.Color that represents the color to display transparently on
    //     the form.
    [SRCategory("CatWindowStyle")]
    [SRDescription("FormTransparencyKeyDescr")]
    public Color TransparencyKey
    {
      get
      {
        object @object = base.Properties.GetObject(PropTransparencyKey);
        if(@object!=null)
        {
          return (Color)@object;
        }

        return Color.Empty;
      }
      set
      {
        base.Properties.SetObject(PropTransparencyKey,value);
        if(!IsMdiContainer)
        {
          bool flag = formState[FormStateLayered]==1;
          if(value!=Color.Empty)
          {
            IntSecurity.TransparentWindows.Demand();
            AllowTransparency=true;
            formState[FormStateLayered]=1;
          }
          else
          {
            formState[FormStateLayered]=((OpacityAsByte<byte.MaxValue) ? 1 : 0);
          }

          if(flag!=(formState[FormStateLayered]!=0))
          {
            UpdateStyles();
          }

          UpdateLayered();
        }
      }
    }

    //
    // Summary:
    //     Gets or sets a value that indicates whether form is minimized, maximized, or
    //     normal.
    //
    // Returns:
    //     A System.Windows.Forms.FormWindowState that represents whether form is minimized,
    //     maximized, or normal. The default is FormWindowState.Normal.
    //
    // Exceptions:
    //   T:System.ComponentModel.InvalidEnumArgumentException:
    //     The value specified is outside the range of valid values.
    [SRCategory("CatLayout")]
    [DefaultValue(FormWindowState.Normal)]
    [SRDescription("FormWindowStateDescr")]
    public FormWindowState WindowState
    {
      get
      {
        return (FormWindowState)formState[FormStateWindowState];
      }
      set
      {
        if(!ClientUtils.IsEnumValid(value,(int)value,0,2))
        {
          throw new InvalidEnumArgumentException("value",(int)value,typeof(FormWindowState));
        }

        if(TopLevel&&IsRestrictedWindow&&value!=0)
        {
          return;
        }

        switch(value)
        {
          case FormWindowState.Normal:
            SetState(65536,value: false);
            break;
          case FormWindowState.Minimized:
          case FormWindowState.Maximized:
            SetState(65536,value: true);
            break;
        }

        if(base.IsHandleCreated&&base.Visible)
        {
          IntPtr handle = base.Handle;
          switch(value)
          {
            case FormWindowState.Normal:
              SafeNativeMethods.ShowWindow(new HandleRef(this,handle),1);
              break;
            case FormWindowState.Maximized:
              SafeNativeMethods.ShowWindow(new HandleRef(this,handle),3);
              break;
            case FormWindowState.Minimized:
              SafeNativeMethods.ShowWindow(new HandleRef(this,handle),6);
              break;
          }
        }

        formState[FormStateWindowState]=(int)value;
      }
    }

    internal override string WindowText
    {
      get
      {
        if(IsRestrictedWindow&&formState[FormStateIsWindowActivated]==1)
        {
          if(userWindowText==null)
          {
            return "";
          }

          return userWindowText;
        }

        return base.WindowText;
      }
      set
      {
        string windowText = WindowText;
        userWindowText=value;
        if(IsRestrictedWindow&&formState[FormStateIsWindowActivated]==1)
        {
          if(value==null)
          {
            value="";
          }

          base.WindowText=RestrictedWindowText(value);
        }
        else
        {
          base.WindowText=value;
        }

        if(windowText==null||windowText.Length==0||value==null||value.Length==0)
        {
          UpdateFormStyles();
        }
      }
    }

    [EditorBrowsable(EditorBrowsableState.Always)]
    [Browsable(true)]
    [SRCategory("CatPropertyChanged")]
    [SRDescription("ControlOnAutoSizeChangedDescr")]
    public new event EventHandler AutoSizeChanged
    {
      add
      {
        base.AutoSizeChanged+=value;
      }
      remove
      {
        base.AutoSizeChanged-=value;
      }
    }

    [Browsable(true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public new event EventHandler AutoValidateChanged
    {
      add
      {
        base.AutoValidateChanged+=value;
      }
      remove
      {
        base.AutoValidateChanged-=value;
      }
    }

    //
    // Summary:
    //     Occurs when the Help button is clicked.
    [EditorBrowsable(EditorBrowsableState.Always)]
    [SRDescription("FormHelpButtonClickedDescr")]
    [Browsable(true)]
    [SRCategory("CatBehavior")]
    public event CancelEventHandler HelpButtonClicked
    {
      add
      {
        base.Events.AddHandler(EVENT_HELPBUTTONCLICKED,value);
      }
      remove
      {
        base.Events.RemoveHandler(EVENT_HELPBUTTONCLICKED,value);
      }
    }

    //
    // Summary:
    //     Occurs when the value of the System.Windows.Forms.Form.MaximizedBounds property
    //     has changed.
    [SRDescription("FormOnMaximizedBoundsChangedDescr")]
    [SRCategory("CatPropertyChanged")]
    public event EventHandler MaximizedBoundsChanged
    {
      add
      {
        base.Events.AddHandler(EVENT_MAXIMIZEDBOUNDSCHANGED,value);
      }
      remove
      {
        base.Events.RemoveHandler(EVENT_MAXIMIZEDBOUNDSCHANGED,value);
      }
    }

    //
    // Summary:
    //     Occurs when the value of the System.Windows.Forms.Form.MaximumSize property has
    //     changed.
    [SRCategory("CatPropertyChanged")]
    [SRDescription("FormOnMaximumSizeChangedDescr")]
    public event EventHandler MaximumSizeChanged
    {
      add
      {
        base.Events.AddHandler(EVENT_MAXIMUMSIZECHANGED,value);
      }
      remove
      {
        base.Events.RemoveHandler(EVENT_MAXIMUMSIZECHANGED,value);
      }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new event EventHandler MarginChanged
    {
      add
      {
        base.MarginChanged+=value;
      }
      remove
      {
        base.MarginChanged-=value;
      }
    }

    //
    // Summary:
    //     Occurs when the value of the System.Windows.Forms.Form.MinimumSize property has
    //     changed.
    [SRDescription("FormOnMinimumSizeChangedDescr")]
    [SRCategory("CatPropertyChanged")]
    public event EventHandler MinimumSizeChanged
    {
      add
      {
        base.Events.AddHandler(EVENT_MINIMUMSIZECHANGED,value);
      }
      remove
      {
        base.Events.RemoveHandler(EVENT_MINIMUMSIZECHANGED,value);
      }
    }

    //
    // Summary:
    //     Occurs when the value of the System.Windows.Forms.Form.TabIndex property changes.
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new event EventHandler TabIndexChanged
    {
      add
      {
        base.TabIndexChanged+=value;
      }
      remove
      {
        base.TabIndexChanged-=value;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new event EventHandler TabStopChanged
    {
      add
      {
        base.TabStopChanged+=value;
      }
      remove
      {
        base.TabStopChanged-=value;
      }
    }

    //
    // Summary:
    //     Occurs when the form is activated in code or by the user.
    [SRCategory("CatFocus")]
    [SRDescription("FormOnActivateDescr")]
    public event EventHandler Activated
    {
      add
      {
        base.Events.AddHandler(EVENT_ACTIVATED,value);
      }
      remove
      {
        base.Events.RemoveHandler(EVENT_ACTIVATED,value);
      }
    }

    //
    // Summary:
    //     Occurs when the form is closing.
    [SRCategory("CatBehavior")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SRDescription("FormOnClosingDescr")]
    [Browsable(false)]
    public event CancelEventHandler Closing
    {
      add
      {
        base.Events.AddHandler(EVENT_CLOSING,value);
      }
      remove
      {
        base.Events.RemoveHandler(EVENT_CLOSING,value);
      }
    }

    //
    // Summary:
    //     Occurs when the form is closed.
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SRCategory("CatBehavior")]
    [SRDescription("FormOnClosedDescr")]
    [Browsable(false)]
    public event EventHandler Closed
    {
      add
      {
        base.Events.AddHandler(EVENT_CLOSED,value);
      }
      remove
      {
        base.Events.RemoveHandler(EVENT_CLOSED,value);
      }
    }

    //
    // Summary:
    //     Occurs when the form loses focus and is no longer the active form.
    [SRDescription("FormOnDeactivateDescr")]
    [SRCategory("CatFocus")]
    public event EventHandler Deactivate
    {
      add
      {
        base.Events.AddHandler(EVENT_DEACTIVATE,value);
      }
      remove
      {
        base.Events.RemoveHandler(EVENT_DEACTIVATE,value);
      }
    }

    //
    // Summary:
    //     Occurs before the form is closed.
    [SRDescription("FormOnFormClosingDescr")]
    [SRCategory("CatBehavior")]
    public event FormClosingEventHandler FormClosing
    {
      add
      {
        base.Events.AddHandler(EVENT_FORMCLOSING,value);
      }
      remove
      {
        base.Events.RemoveHandler(EVENT_FORMCLOSING,value);
      }
    }

    //
    // Summary:
    //     Occurs after the form is closed.
    [SRCategory("CatBehavior")]
    [SRDescription("FormOnFormClosedDescr")]
    public event FormClosedEventHandler FormClosed
    {
      add
      {
        base.Events.AddHandler(EVENT_FORMCLOSED,value);
      }
      remove
      {
        base.Events.RemoveHandler(EVENT_FORMCLOSED,value);
      }
    }

    //
    // Summary:
    //     Occurs before a form is displayed for the first time.
    [SRDescription("FormOnLoadDescr")]
    [SRCategory("CatBehavior")]
    public event EventHandler Load
    {
      add
      {
        base.Events.AddHandler(EVENT_LOAD,value);
      }
      remove
      {
        base.Events.RemoveHandler(EVENT_LOAD,value);
      }
    }

    //
    // Summary:
    //     Occurs when a multiple-document interface (MDI) child form is activated or closed
    //     within an MDI application.
    [SRCategory("CatLayout")]
    [SRDescription("FormOnMDIChildActivateDescr")]
    public event EventHandler MdiChildActivate
    {
      add
      {
        base.Events.AddHandler(EVENT_MDI_CHILD_ACTIVATE,value);
      }
      remove
      {
        base.Events.RemoveHandler(EVENT_MDI_CHILD_ACTIVATE,value);
      }
    }

    //
    // Summary:
    //     Occurs when the menu of a form loses focus.
    [Browsable(false)]
    [SRCategory("CatBehavior")]
    [SRDescription("FormOnMenuCompleteDescr")]
    public event EventHandler MenuComplete
    {
      add
      {
        base.Events.AddHandler(EVENT_MENUCOMPLETE,value);
      }
      remove
      {
        base.Events.RemoveHandler(EVENT_MENUCOMPLETE,value);
      }
    }

    //
    // Summary:
    //     Occurs when the menu of a form receives focus.
    [Browsable(false)]
    [SRCategory("CatBehavior")]
    [SRDescription("FormOnMenuStartDescr")]
    public event EventHandler MenuStart
    {
      add
      {
        base.Events.AddHandler(EVENT_MENUSTART,value);
      }
      remove
      {
        base.Events.RemoveHandler(EVENT_MENUSTART,value);
      }
    }

    //
    // Summary:
    //     Occurs after the input language of the form has changed.
    [SRDescription("FormOnInputLangChangeDescr")]
    [SRCategory("CatBehavior")]
    public event InputLanguageChangedEventHandler InputLanguageChanged
    {
      add
      {
        base.Events.AddHandler(EVENT_INPUTLANGCHANGE,value);
      }
      remove
      {
        base.Events.RemoveHandler(EVENT_INPUTLANGCHANGE,value);
      }
    }

    //
    // Summary:
    //     Occurs when the user attempts to change the input language for the form.
    [SRCategory("CatBehavior")]
    [SRDescription("FormOnInputLangChangeRequestDescr")]
    public event InputLanguageChangingEventHandler InputLanguageChanging
    {
      add
      {
        base.Events.AddHandler(EVENT_INPUTLANGCHANGEREQUEST,value);
      }
      remove
      {
        base.Events.RemoveHandler(EVENT_INPUTLANGCHANGEREQUEST,value);
      }
    }

    //
    // Summary:
    //     Occurs after the value of the System.Windows.Forms.Form.RightToLeftLayout property
    //     changes.
    [SRDescription("ControlOnRightToLeftLayoutChangedDescr")]
    [SRCategory("CatPropertyChanged")]
    public event EventHandler RightToLeftLayoutChanged
    {
      add
      {
        base.Events.AddHandler(EVENT_RIGHTTOLEFTLAYOUTCHANGED,value);
      }
      remove
      {
        base.Events.RemoveHandler(EVENT_RIGHTTOLEFTLAYOUTCHANGED,value);
      }
    }

    //
    // Summary:
    //     Occurs whenever the form is first displayed.
    [SRDescription("FormOnShownDescr")]
    [SRCategory("CatBehavior")]
    public event EventHandler Shown
    {
      add
      {
        base.Events.AddHandler(EVENT_SHOWN,value);
      }
      remove
      {
        base.Events.RemoveHandler(EVENT_SHOWN,value);
      }
    }

    //
    // Summary:
    //     Occurs when a form enters resizing mode.
    [SRCategory("CatAction")]
    [SRDescription("FormOnResizeBeginDescr")]
    public event EventHandler ResizeBegin
    {
      add
      {
        base.Events.AddHandler(EVENT_RESIZEBEGIN,value);
      }
      remove
      {
        base.Events.RemoveHandler(EVENT_RESIZEBEGIN,value);
      }
    }

    //
    // Summary:
    //     Occurs when a form exits resizing mode.
    [SRCategory("CatAction")]
    [SRDescription("FormOnResizeEndDescr")]
    public event EventHandler ResizeEnd
    {
      add
      {
        base.Events.AddHandler(EVENT_RESIZEEND,value);
      }
      remove
      {
        base.Events.RemoveHandler(EVENT_RESIZEEND,value);
      }
    }

    //
    // Summary:
    //     Initializes a new instance of the System.Windows.Forms.Form class.
    public Form()
    {
      _=IsRestrictedWindow;
      formStateEx[FormStateExShowIcon]=1;
      SetState(2,value: false);
      SetState(524288,value: true);
    }

    //
    // Parameters:
    //   value:
    //     true to make the control visible; otherwise, false.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected override void SetVisibleCore(bool value)
    {
      if(GetVisibleCore()==value&&dialogResult==DialogResult.OK)
      {
        return;
      }

      if(GetVisibleCore()==value&&(!value||CalledMakeVisible))
      {
        base.SetVisibleCore(value);
        return;
      }

      if(value)
      {
        CalledMakeVisible=true;
        if(CalledCreateControl)
        {
          if(CalledOnLoad)
          {
            if(!Application.OpenFormsInternal.Contains(this))
            {
              Application.OpenFormsInternalAdd(this);
            }
          }
          else
          {
            CalledOnLoad=true;
            OnLoad(EventArgs.Empty);
            if(dialogResult!=0)
            {
              value=false;
            }
          }
        }
      }
      else
      {
        ResetSecurityTip(modalOnly: true);
      }

      if(!IsMdiChild)
      {
        base.SetVisibleCore(value);
        if(formState[FormStateSWCalled]==0)
        {
          UnsafeNativeMethods.SendMessage(new HandleRef(this,base.Handle),24,value ? 1 : 0,0);
        }
      }
      else
      {
        if(base.IsHandleCreated)
        {
          DestroyHandle();
        }

        if(!value)
        {
          InvalidateMergedMenu();
          SetState(2,value: false);
        }
        else
        {
          SetState(2,value: true);
          MdiParentInternal.MdiClient.PerformLayout();
          if(ParentInternal!=null&&ParentInternal.Visible)
          {
            SuspendLayout();
            try
            {
              SafeNativeMethods.ShowWindow(new HandleRef(this,base.Handle),5);
              CreateControl();
              if(WindowState==FormWindowState.Maximized)
              {
                MdiParentInternal.UpdateWindowIcon(redrawFrame: true);
              }
            }
            finally
            {
              ResumeLayout();
            }
          }
        }

        OnVisibleChanged(EventArgs.Empty);
      }

      if(value&&!IsMdiChild&&(WindowState==FormWindowState.Maximized||TopMost))
      {
        if(base.ActiveControl==null)
        {
          SelectNextControlInternal(null,forward: true,tabStopOnly: true,nested: true,wrap: false);
        }

        FocusActiveControlInternal();
      }
    }

    //
    // Summary:
    //     Activates the form and gives it focus.
    public void Activate()
    {
      IntSecurity.ModifyFocus.Demand();
      if(base.Visible&&base.IsHandleCreated)
      {
        if(IsMdiChild)
        {
          MdiParentInternal.MdiClient.SendMessage(546,base.Handle,0);
        }
        else
        {
          UnsafeNativeMethods.SetForegroundWindow(new HandleRef(this,base.Handle));
        }
      }
    }

    //
    // Summary:
    //     Activates the MDI child of a form.
    //
    // Parameters:
    //   form:
    //     The child form to activate.
    protected void ActivateMdiChild(Form form)
    {
      IntSecurity.ModifyFocus.Demand();
      ActivateMdiChildInternal(form);
    }

    private void ActivateMdiChildInternal(Form form)
    {
      if(FormerlyActiveMdiChild!=null&&!FormerlyActiveMdiChild.IsClosing)
      {
        FormerlyActiveMdiChild.UpdateWindowIcon(redrawFrame: true);
        FormerlyActiveMdiChild=null;
      }

      Form activeMdiChildInternal = ActiveMdiChildInternal;
      if(activeMdiChildInternal!=form)
      {
        if(activeMdiChildInternal!=null)
        {
          activeMdiChildInternal.Active=false;
        }

        activeMdiChildInternal=form;
        ActiveMdiChildInternal=form;
        if(activeMdiChildInternal!=null)
        {
          activeMdiChildInternal.IsMdiChildFocusable=true;
          activeMdiChildInternal.Active=true;
        }
        else if(Active)
        {
          ActivateControlInternal(this);
        }

        OnMdiChildActivate(EventArgs.Empty);
      }
    }

    //
    // Summary:
    //     Adds an owned form to this form.
    //
    // Parameters:
    //   ownedForm:
    //     The System.Windows.Forms.Form that this form will own.
    public void AddOwnedForm(Form ownedForm)
    {
      if(ownedForm==null)
      {
        return;
      }

      if(ownedForm.OwnerInternal!=this)
      {
        ownedForm.Owner=this;
        return;
      }

      Form[] array = (Form[])base.Properties.GetObject(PropOwnedForms);
      int integer = base.Properties.GetInteger(PropOwnedFormsCount);
      for(int i = 0;i<integer;i++)
      {
        if(array[i]==ownedForm)
        {
          return;
        }
      }

      if(array==null)
      {
        array=new Form[4];
        base.Properties.SetObject(PropOwnedForms,array);
      }
      else if(array.Length==integer)
      {
        Form[] array2 = new Form[integer*2];
        Array.Copy(array,0,array2,0,integer);
        array=array2;
        base.Properties.SetObject(PropOwnedForms,array);
      }

      array[integer]=ownedForm;
      base.Properties.SetInteger(PropOwnedFormsCount,integer+1);
    }

    private float AdjustScale(float scale)
    {
      if(scale<0.92f)
      {
        return scale+0.08f;
      }

      if(scale<1f)
      {
        return 1f;
      }

      if(scale>1.01f)
      {
        return scale+0.08f;
      }

      return scale;
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected override void AdjustFormScrollbars(bool displayScrollbars)
    {
      if(WindowState!=FormWindowState.Minimized)
      {
        base.AdjustFormScrollbars(displayScrollbars);
      }
    }

    private void AdjustSystemMenu(IntPtr hmenu)
    {
      UpdateWindowState();
      FormWindowState windowState = WindowState;
      FormBorderStyle formBorderStyle = FormBorderStyle;
      bool flag = formBorderStyle==FormBorderStyle.SizableToolWindow||formBorderStyle==FormBorderStyle.Sizable;
      bool flag2 = MinimizeBox&&windowState!=FormWindowState.Minimized;
      bool flag3 = MaximizeBox&&windowState!=FormWindowState.Maximized;
      bool controlBox = ControlBox;
      bool flag4 = windowState!=FormWindowState.Normal;
      bool flag5 = flag&&windowState!=FormWindowState.Minimized&&windowState!=FormWindowState.Maximized;
      if(!flag2)
      {
        UnsafeNativeMethods.EnableMenuItem(new HandleRef(this,hmenu),61472,1);
      }
      else
      {
        UnsafeNativeMethods.EnableMenuItem(new HandleRef(this,hmenu),61472,0);
      }

      if(!flag3)
      {
        UnsafeNativeMethods.EnableMenuItem(new HandleRef(this,hmenu),61488,1);
      }
      else
      {
        UnsafeNativeMethods.EnableMenuItem(new HandleRef(this,hmenu),61488,0);
      }

      if(!controlBox)
      {
        UnsafeNativeMethods.EnableMenuItem(new HandleRef(this,hmenu),61536,1);
      }
      else
      {
        UnsafeNativeMethods.EnableMenuItem(new HandleRef(this,hmenu),61536,0);
      }

      if(!flag4)
      {
        UnsafeNativeMethods.EnableMenuItem(new HandleRef(this,hmenu),61728,1);
      }
      else
      {
        UnsafeNativeMethods.EnableMenuItem(new HandleRef(this,hmenu),61728,0);
      }

      if(!flag5)
      {
        UnsafeNativeMethods.EnableMenuItem(new HandleRef(this,hmenu),61440,1);
      }
      else
      {
        UnsafeNativeMethods.EnableMenuItem(new HandleRef(this,hmenu),61440,0);
      }
    }

    private void AdjustSystemMenu()
    {
      if(base.IsHandleCreated)
      {
        IntPtr systemMenu = UnsafeNativeMethods.GetSystemMenu(new HandleRef(this,base.Handle),bRevert: false);
        AdjustSystemMenu(systemMenu);
        systemMenu=IntPtr.Zero;
      }
    }

    //
    // Summary:
    //     Resizes the form according to the current value of the System.Windows.Forms.Form.AutoScaleBaseSize
    //     property and the size of the current font.
    [Obsolete("This method has been deprecated. Use the ApplyAutoScaling method instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected void ApplyAutoScaling()
    {
      if(!autoScaleBaseSize.IsEmpty)
      {
        Size size = AutoScaleBaseSize;
        SizeF autoScaleSize = GetAutoScaleSize(Font);
        Size size2 = new Size((int)Math.Round(autoScaleSize.Width),(int)Math.Round(autoScaleSize.Height));
        if(!size.Equals(size2))
        {
          float dy = AdjustScale((float)size2.Height/(float)size.Height);
          float dx = AdjustScale((float)size2.Width/(float)size.Width);
          Scale(dx,dy);
          AutoScaleBaseSize=size2;
        }
      }
    }

    private void ApplyClientSize()
    {
      if(formState[FormStateWindowState]!=0||!base.IsHandleCreated)
      {
        return;
      }

      Size clientSize = ClientSize;
      bool hScroll = base.HScroll;
      bool vScroll = base.VScroll;
      bool flag = false;
      if(formState[FormStateSetClientSize]!=0)
      {
        flag=true;
        formState[FormStateSetClientSize]=0;
      }

      if(flag)
      {
        if(hScroll)
        {
          clientSize.Height+=SystemInformation.HorizontalScrollBarHeight;
        }

        if(vScroll)
        {
          clientSize.Width+=SystemInformation.VerticalScrollBarWidth;
        }
      }

      IntPtr handle = base.Handle;
      NativeMethods.RECT rect = default(NativeMethods.RECT);
      SafeNativeMethods.GetClientRect(new HandleRef(this,handle),ref rect);
      Rectangle rectangle = Rectangle.FromLTRB(rect.left,rect.top,rect.right,rect.bottom);
      Rectangle bounds = base.Bounds;
      if(clientSize.Width!=rectangle.Width)
      {
        Size size = ComputeWindowSize(clientSize);
        if(vScroll)
        {
          size.Width+=SystemInformation.VerticalScrollBarWidth;
        }

        if(hScroll)
        {
          size.Height+=SystemInformation.HorizontalScrollBarHeight;
        }

        bounds.Width=size.Width;
        bounds.Height=size.Height;
        base.Bounds=bounds;
        SafeNativeMethods.GetClientRect(new HandleRef(this,handle),ref rect);
        rectangle=Rectangle.FromLTRB(rect.left,rect.top,rect.right,rect.bottom);
      }

      if(clientSize.Height!=rectangle.Height)
      {
        int num = clientSize.Height-rectangle.Height;
        bounds.Height+=num;
        base.Bounds=bounds;
      }

      UpdateBounds();
    }

    internal override void AssignParent(Control value)
    {
      Form form = (Form)base.Properties.GetObject(PropFormMdiParent);
      if(form!=null&&form.MdiClient!=value)
      {
        base.Properties.SetObject(PropFormMdiParent,null);
      }

      base.AssignParent(value);
    }

    internal bool CheckCloseDialog(bool closingOnly)
    {
      if(dialogResult==DialogResult.None&&base.Visible)
      {
        return false;
      }

      try
      {
        FormClosingEventArgs formClosingEventArgs = new FormClosingEventArgs(closeReason,cancel: false);
        if(!CalledClosing)
        {
          OnClosing(formClosingEventArgs);
          OnFormClosing(formClosingEventArgs);
          if(formClosingEventArgs.Cancel)
          {
            dialogResult=DialogResult.None;
          }
          else
          {
            CalledClosing=true;
          }
        }

        if(!closingOnly&&dialogResult!=0)
        {
          FormClosedEventArgs e = new FormClosedEventArgs(closeReason);
          OnClosed(e);
          OnFormClosed(e);
          CalledClosing=false;
        }
      }
      catch(Exception t)
      {
        dialogResult=DialogResult.None;
        if(NativeWindow.WndProcShouldBeDebuggable)
        {
          throw;
        }

        Application.OnThreadException(t);
      }

      if(dialogResult==DialogResult.None)
      {
        return !base.Visible;
      }

      return true;
    }

    //
    // Summary:
    //     Closes the form.
    //
    // Exceptions:
    //   T:System.InvalidOperationException:
    //     The form was closed while a handle was being created.
    //
    //   T:System.ObjectDisposedException:
    //     You cannot call this method from the System.Windows.Forms.Form.Activated event
    //     when System.Windows.Forms.Form.WindowState is set to System.Windows.Forms.FormWindowState.Maximized.
    public void Close()
    {
      if(GetState(262144))
      {
        throw new InvalidOperationException(SR.GetString("ClosingWhileCreatingHandle","Close"));
      }

      if(base.IsHandleCreated)
      {
        closeReason=CloseReason.UserClosing;
        SendMessage(16,0,0);
      }
      else
      {
        Dispose();
      }
    }

    private Size ComputeWindowSize(Size clientSize)
    {
      CreateParams createParams = CreateParams;
      return ComputeWindowSize(clientSize,createParams.Style,createParams.ExStyle);
    }

    private Size ComputeWindowSize(Size clientSize,int style,int exStyle)
    {
      NativeMethods.RECT lpRect = new NativeMethods.RECT(0,0,clientSize.Width,clientSize.Height);
      SafeNativeMethods.AdjustWindowRectEx(ref lpRect,style,HasMenu,exStyle);
      return new Size(lpRect.right-lpRect.left,lpRect.bottom-lpRect.top);
    }

    //
    // Returns:
    //     A new instance of System.Windows.Forms.Control.ControlCollection assigned to
    //     the control.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected override Control.ControlCollection CreateControlsInstance()
    {
      return new ControlCollection(this);
    }

    internal override void AfterControlRemoved(Control control,Control oldParent)
    {
      base.AfterControlRemoved(control,oldParent);
      if(control==AcceptButton)
      {
        AcceptButton=null;
      }

      if(control==CancelButton)
      {
        CancelButton=null;
      }

      if(control==ctlClient)
      {
        ctlClient=null;
        UpdateMenuHandles();
      }
    }

    //
    // Summary:
    //     Creates the handle for the form. If a derived class overrides this function,
    //     it must call the base implementation.
    //
    // Exceptions:
    //   T:System.InvalidOperationException:
    //     A handle for this System.Windows.Forms.Form has already been created.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected override void CreateHandle()
    {
      Form form = (Form)base.Properties.GetObject(PropFormMdiParent);
      form?.SuspendUpdateMenuHandles();
      try
      {
        if(IsMdiChild&&MdiParentInternal.IsHandleCreated)
        {
          MdiClient mdiClient = MdiParentInternal.MdiClient;
          if(mdiClient!=null&&!mdiClient.IsHandleCreated)
          {
            mdiClient.CreateControl();
          }
        }

        if(IsMdiChild&&formState[FormStateWindowState]==2)
        {
          formState[FormStateWindowState]=0;
          formState[FormStateMdiChildMax]=1;
          base.CreateHandle();
          formState[FormStateWindowState]=2;
          formState[FormStateMdiChildMax]=0;
        }
        else
        {
          base.CreateHandle();
        }

        UpdateHandleWithOwner();
        UpdateWindowIcon(redrawFrame: false);
        AdjustSystemMenu();
        if(formState[FormStateStartPos]!=3)
        {
          ApplyClientSize();
        }

        if(formState[FormStateShowWindowOnCreate]==1)
        {
          base.Visible=true;
        }

        if(Menu!=null||!TopLevel||IsMdiContainer)
        {
          UpdateMenuHandles();
        }

        if(!ShowInTaskbar&&OwnerInternal==null&&TopLevel)
        {
          UnsafeNativeMethods.SetWindowLong(new HandleRef(this,base.Handle),-8,TaskbarOwner);
          Icon icon = Icon;
          if(icon!=null&&TaskbarOwner.Handle!=IntPtr.Zero)
          {
            UnsafeNativeMethods.SendMessage(TaskbarOwner,128,1,icon.Handle);
          }
        }

        if(formState[FormStateTopMost]!=0)
        {
          TopMost=true;
        }
      }
      finally
      {
        form?.ResumeUpdateMenuHandles();
        UpdateStyles();
      }
    }

    private void DeactivateMdiChild()
    {
      Form activeMdiChildInternal = ActiveMdiChildInternal;
      if(activeMdiChildInternal==null)
      {
        return;
      }

      Form mdiParentInternal = activeMdiChildInternal.MdiParentInternal;
      activeMdiChildInternal.Active=false;
      activeMdiChildInternal.IsMdiChildFocusable=false;
      FormerlyActiveMdiChild=activeMdiChildInternal;
      bool flag = true;
      Form[] mdiChildren = mdiParentInternal.MdiChildren;
      foreach(Form form in mdiChildren)
      {
        if(form!=this&&form.Visible)
        {
          flag=false;
          break;
        }
      }

      if(flag)
      {
        mdiParentInternal.ActivateMdiChildInternal(null);
      }

      ActiveMdiChildInternal=null;
      UpdateMenuHandles();
      UpdateToolStrip();
    }

    //
    // Parameters:
    //   m:
    //     The Windows System.Windows.Forms.Message to process.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [SecurityPermission(SecurityAction.LinkDemand,Flags = SecurityPermissionFlag.UnmanagedCode)]
    protected override void DefWndProc(ref Message m)
    {
      if(ctlClient!=null&&ctlClient.IsHandleCreated&&ctlClient.ParentInternal==this)
      {
        m.Result=UnsafeNativeMethods.DefFrameProc(m.HWnd,ctlClient.Handle,m.Msg,m.WParam,m.LParam);
      }
      else if(formStateEx[FormStateExUseMdiChildProc]!=0)
      {
        m.Result=UnsafeNativeMethods.DefMDIChildProc(m.HWnd,m.Msg,m.WParam,m.LParam);
      }
      else
      {
        base.DefWndProc(ref m);
      }
    }

    //
    // Summary:
    //     Disposes of the resources (other than memory) used by the System.Windows.Forms.Form.
    //
    // Parameters:
    //   disposing:
    //     true to release both managed and unmanaged resources; false to release only unmanaged
    //     resources.
    protected override void Dispose(bool disposing)
    {
      if(disposing)
      {
        CalledOnLoad=false;
        CalledMakeVisible=false;
        CalledCreateControl=false;
        if(base.Properties.ContainsObject(PropAcceptButton))
        {
          base.Properties.SetObject(PropAcceptButton,null);
        }

        if(base.Properties.ContainsObject(PropCancelButton))
        {
          base.Properties.SetObject(PropCancelButton,null);
        }

        if(base.Properties.ContainsObject(PropDefaultButton))
        {
          base.Properties.SetObject(PropDefaultButton,null);
        }

        if(base.Properties.ContainsObject(PropActiveMdiChild))
        {
          base.Properties.SetObject(PropActiveMdiChild,null);
        }

        if(MdiWindowListStrip!=null)
        {
          MdiWindowListStrip.Dispose();
          MdiWindowListStrip=null;
        }

        if(MdiControlStrip!=null)
        {
          MdiControlStrip.Dispose();
          MdiControlStrip=null;
        }

        if(MainMenuStrip!=null)
        {
          MainMenuStrip=null;
        }

        Form form = (Form)base.Properties.GetObject(PropOwner);
        if(form!=null)
        {
          form.RemoveOwnedForm(this);
          base.Properties.SetObject(PropOwner,null);
        }

        Form[] array = (Form[])base.Properties.GetObject(PropOwnedForms);
        int integer = base.Properties.GetInteger(PropOwnedFormsCount);
        for(int num = integer-1;num>=0;num--)
        {
          if(array[num]!=null)
          {
            array[num].Dispose();
          }
        }

        if(smallIcon!=null)
        {
          smallIcon.Dispose();
          smallIcon=null;
        }

        ResetSecurityTip(modalOnly: false);
        base.Dispose(disposing);
        ctlClient=null;
        MainMenu menu = Menu;
        if(menu!=null&&menu.ownerForm==this)
        {
          menu.Dispose();
          base.Properties.SetObject(PropMainMenu,null);
        }

        if(base.Properties.GetObject(PropCurMenu)!=null)
        {
          base.Properties.SetObject(PropCurMenu,null);
        }

        MenuChanged(0,null);
        MainMenu mainMenu = (MainMenu)base.Properties.GetObject(PropDummyMenu);
        if(mainMenu!=null)
        {
          mainMenu.Dispose();
          base.Properties.SetObject(PropDummyMenu,null);
        }

        MainMenu mainMenu2 = (MainMenu)base.Properties.GetObject(PropMergedMenu);
        if(mainMenu2!=null)
        {
          if(mainMenu2.ownerForm==this||mainMenu2.form==null)
          {
            mainMenu2.Dispose();
          }

          base.Properties.SetObject(PropMergedMenu,null);
        }
      }
      else
      {
        base.Dispose(disposing);
      }
    }

    private void FillInCreateParamsBorderIcons(CreateParams cp)
    {
      if(FormBorderStyle!=0)
      {
        if(Text!=null&&Text.Length!=0)
        {
          cp.Style|=12582912;
        }

        if(ControlBox||IsRestrictedWindow)
        {
          cp.Style|=13107200;
        }
        else
        {
          cp.Style&=-524289;
        }

        if(MaximizeBox||IsRestrictedWindow)
        {
          cp.Style|=65536;
        }
        else
        {
          cp.Style&=-65537;
        }

        if(MinimizeBox||IsRestrictedWindow)
        {
          cp.Style|=131072;
        }
        else
        {
          cp.Style&=-131073;
        }

        if(HelpButton&&!MaximizeBox&&!MinimizeBox&&ControlBox)
        {
          cp.ExStyle|=1024;
        }
        else
        {
          cp.ExStyle&=-1025;
        }
      }
    }

    private void FillInCreateParamsBorderStyles(CreateParams cp)
    {
      switch(formState[FormStateBorderStyle])
      {
        case 0:
          if(!IsRestrictedWindow)
          {
            break;
          }

          goto case 1;
        case 1:
          cp.Style|=8388608;
          break;
        case 4:
          cp.Style|=8650752;
          break;
        case 2:
          cp.Style|=8388608;
          cp.ExStyle|=512;
          break;
        case 3:
          cp.Style|=8388608;
          cp.ExStyle|=1;
          break;
        case 5:
          cp.Style|=8388608;
          cp.ExStyle|=128;
          break;
        case 6:
          cp.Style|=8650752;
          cp.ExStyle|=128;
          break;
      }
    }

    private void FillInCreateParamsStartPosition(CreateParams cp)
    {
      if(formState[FormStateSetClientSize]!=0)
      {
        int style = cp.Style&-553648129;
        Size size = ComputeWindowSize(ClientSize,style,cp.ExStyle);
        if(IsRestrictedWindow)
        {
          size=ApplyBoundsConstraints(cp.X,cp.Y,size.Width,size.Height).Size;
        }

        cp.Width=size.Width;
        cp.Height=size.Height;
      }

      switch(formState[FormStateStartPos])
      {
        case 3:
          cp.Width=int.MinValue;
          cp.Height=int.MinValue;
          goto case 2;
        case 2:
        case 4:
          if(!IsMdiChild||Dock==DockStyle.None)
          {
            cp.X=int.MinValue;
            cp.Y=int.MinValue;
          }

          break;
        case 1:
        {
          if(IsMdiChild)
          {
            Control mdiClient = MdiParentInternal.MdiClient;
            Rectangle clientRectangle = mdiClient.ClientRectangle;
            cp.X=Math.Max(clientRectangle.X,clientRectangle.X+(clientRectangle.Width-cp.Width)/2);
            cp.Y=Math.Max(clientRectangle.Y,clientRectangle.Y+(clientRectangle.Height-cp.Height)/2);
            break;
          }

          Screen screen = null;
          IWin32Window win32Window = (IWin32Window)base.Properties.GetObject(PropDialogOwner);
          if(OwnerInternal!=null||win32Window!=null)
          {
            IntPtr hwnd = ((win32Window!=null) ? Control.GetSafeHandle(win32Window) : OwnerInternal.Handle);
            screen=Screen.FromHandleInternal(hwnd);
          }
          else
          {
            screen=Screen.FromPoint(Control.MousePosition);
          }

          Rectangle workingArea = screen.WorkingArea;
          if(WindowState!=FormWindowState.Maximized)
          {
            cp.X=Math.Max(workingArea.X,workingArea.X+(workingArea.Width-cp.Width)/2);
            cp.Y=Math.Max(workingArea.Y,workingArea.Y+(workingArea.Height-cp.Height)/2);
          }

          break;
        }
      }
    }

    private void FillInCreateParamsWindowState(CreateParams cp)
    {
      switch(formState[FormStateWindowState])
      {
        case 2:
          cp.Style|=16777216;
          break;
        case 1:
          cp.Style|=536870912;
          break;
      }
    }

    internal override bool FocusInternal()
    {
      if(IsMdiChild)
      {
        MdiParentInternal.MdiClient.SendMessage(546,base.Handle,0);
        return Focused;
      }

      return base.FocusInternal();
    }

    //
    // Summary:
    //     Gets the size when autoscaling the form based on a specified font.
    //
    // Parameters:
    //   font:
    //     A System.Drawing.Font representing the font to determine the autoscaled base
    //     size of the form.
    //
    // Returns:
    //     A System.Drawing.SizeF representing the autoscaled size of the form.
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This method has been deprecated. Use the AutoScaleDimensions property instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
    public static SizeF GetAutoScaleSize(Font font)
    {
      float num = font.Height;
      float num2 = 9f;
      try
      {
        using Graphics graphics = Graphics.FromHwndInternal(IntPtr.Zero);
        string text = "The quick brown fox jumped over the lazy dog.";
        double num3 = 44.549996948242189;
        float num4 = graphics.MeasureString(text,font).Width;
        num2=(float)((double)num4/num3);
      }
      catch
      {
      }

      return new SizeF(num2,num);
    }

    internal override Size GetPreferredSizeCore(Size proposedSize)
    {
      return base.GetPreferredSizeCore(proposedSize);
    }

    private void ResolveZoneAndSiteNames(ArrayList sites,ref string securityZone,ref string securitySite)
    {
      securityZone=SR.GetString("SecurityRestrictedWindowTextUnknownZone");
      securitySite=SR.GetString("SecurityRestrictedWindowTextUnknownSite");
      try
      {
        if(sites==null||sites.Count==0)
        {
          return;
        }

        ArrayList arrayList = new ArrayList();
        foreach(object site in sites)
        {
          if(site==null)
          {
            return;
          }

          string text = site.ToString();
          if(text.Length==0)
          {
            return;
          }

          Zone zone = Zone.CreateFromUrl(text);
          if(!zone.SecurityZone.Equals(SecurityZone.MyComputer))
          {
            string text2 = zone.SecurityZone.ToString();
            if(!arrayList.Contains(text2))
            {
              arrayList.Add(text2);
            }
          }
        }

        if(arrayList.Count==0)
        {
          securityZone=SecurityZone.MyComputer.ToString();
        }
        else if(arrayList.Count==1)
        {
          securityZone=arrayList[0].ToString();
        }
        else
        {
          securityZone=SR.GetString("SecurityRestrictedWindowTextMixedZone");
        }

        ArrayList arrayList2 = new ArrayList();
        FileIOPermission fileIOPermission = new FileIOPermission(PermissionState.None);
        fileIOPermission.AllFiles=FileIOPermissionAccess.PathDiscovery;
        fileIOPermission.Assert();
        try
        {
          Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
          foreach(Assembly assembly in assemblies)
          {
            if(assembly.GlobalAssemblyCache)
            {
              arrayList2.Add(assembly.CodeBase.ToUpper(CultureInfo.InvariantCulture));
            }
          }
        }
        finally
        {
          CodeAccessPermission.RevertAssert();
        }

        ArrayList arrayList3 = new ArrayList();
        foreach(object site2 in sites)
        {
          Uri uri = new Uri(site2.ToString());
          if(!arrayList2.Contains(uri.AbsoluteUri.ToUpper(CultureInfo.InvariantCulture)))
          {
            string host = uri.Host;
            if(host.Length>0&&!arrayList3.Contains(host))
            {
              arrayList3.Add(host);
            }
          }
        }

        if(arrayList3.Count==0)
        {
          new EnvironmentPermission(PermissionState.Unrestricted).Assert();
          try
          {
            securitySite=Environment.MachineName;
          }
          finally
          {
            CodeAccessPermission.RevertAssert();
          }
        }
        else if(arrayList3.Count==1)
        {
          securitySite=arrayList3[0].ToString();
        }
        else
        {
          securitySite=SR.GetString("SecurityRestrictedWindowTextMultipleSites");
        }
      }
      catch
      {
      }
    }

    private string RestrictedWindowText(string original)
    {
      EnsureSecurityInformation();
      return string.Format(CultureInfo.CurrentCulture,Application.SafeTopLevelCaptionFormat,original,securityZone,securitySite);
    }

    private void EnsureSecurityInformation()
    {
      if(securityZone==null||securitySite==null)
      {
        SecurityManager.GetZoneAndOrigin(out var _,out var origin);
        ResolveZoneAndSiteNames(origin,ref securityZone,ref securitySite);
      }
    }

    private void CallShownEvent()
    {
      OnShown(EventArgs.Empty);
    }

    internal override bool CanSelectCore()
    {
      if(!GetStyle(ControlStyles.Selectable)||!base.Enabled||!base.Visible)
      {
        return false;
      }

      return true;
    }

    internal bool CanRecreateHandle()
    {
      if(IsMdiChild)
      {
        if(GetState(2))
        {
          return base.IsHandleCreated;
        }

        return false;
      }

      return true;
    }

    internal override bool CanProcessMnemonic()
    {
      if(IsMdiChild&&(formStateEx[FormStateExMnemonicProcessed]==1||this!=MdiParentInternal.ActiveMdiChildInternal||WindowState==FormWindowState.Minimized))
      {
        return false;
      }

      return base.CanProcessMnemonic();
    }

    [UIPermission(SecurityAction.LinkDemand,Window = UIPermissionWindow.AllWindows)]
    protected internal override bool ProcessMnemonic(char charCode)
    {
      if(base.ProcessMnemonic(charCode))
      {
        return true;
      }

      if(IsMdiContainer)
      {
        if(base.Controls.Count>1)
        {
          for(int i = 0;i<base.Controls.Count;i++)
          {
            Control control = base.Controls[i];
            if(!(control is MdiClient)&&control.ProcessMnemonic(charCode))
            {
              return true;
            }
          }
        }

        return false;
      }

      return false;
    }

    //
    // Summary:
    //     Centers the position of the form within the bounds of the parent form.
    protected void CenterToParent()
    {
      if(!TopLevel)
      {
        return;
      }

      Point location = default(Point);
      Size size = Size;
      IntPtr zero = IntPtr.Zero;
      zero=UnsafeNativeMethods.GetWindowLong(new HandleRef(this,base.Handle),-8);
      if(zero!=IntPtr.Zero)
      {
        Screen screen = Screen.FromHandleInternal(zero);
        Rectangle workingArea = screen.WorkingArea;
        NativeMethods.RECT rect = default(NativeMethods.RECT);
        UnsafeNativeMethods.GetWindowRect(new HandleRef(null,zero),ref rect);
        location.X=(rect.left+rect.right-size.Width)/2;
        if(location.X<workingArea.X)
        {
          location.X=workingArea.X;
        }
        else if(location.X+size.Width>workingArea.X+workingArea.Width)
        {
          location.X=workingArea.X+workingArea.Width-size.Width;
        }

        location.Y=(rect.top+rect.bottom-size.Height)/2;
        if(location.Y<workingArea.Y)
        {
          location.Y=workingArea.Y;
        }
        else if(location.Y+size.Height>workingArea.Y+workingArea.Height)
        {
          location.Y=workingArea.Y+workingArea.Height-size.Height;
        }

        Location=location;
      }
      else
      {
        CenterToScreen();
      }
    }

    //
    // Summary:
    //     Centers the form on the current screen.
    protected void CenterToScreen()
    {
      Point location = default(Point);
      Screen screen = null;
      if(OwnerInternal!=null)
      {
        screen=Screen.FromControl(OwnerInternal);
      }
      else
      {
        IntPtr intPtr = IntPtr.Zero;
        if(TopLevel)
        {
          intPtr=UnsafeNativeMethods.GetWindowLong(new HandleRef(this,base.Handle),-8);
        }

        screen=((!(intPtr!=IntPtr.Zero)) ? Screen.FromPoint(Control.MousePosition) : Screen.FromHandleInternal(intPtr));
      }

      Rectangle workingArea = screen.WorkingArea;
      location.X=Math.Max(workingArea.X,workingArea.X+(workingArea.Width-base.Width)/2);
      location.Y=Math.Max(workingArea.Y,workingArea.Y+(workingArea.Height-base.Height)/2);
      Location=location;
    }

    private void InvalidateMergedMenu()
    {
      if(base.Properties.ContainsObject(PropMergedMenu))
      {
        MainMenu mainMenu = base.Properties.GetObject(PropMergedMenu) as MainMenu;
        if(mainMenu!=null&&mainMenu.ownerForm==this)
        {
          mainMenu.Dispose();
        }

        base.Properties.SetObject(PropMergedMenu,null);
      }

      Form parentFormInternal = base.ParentFormInternal;
      parentFormInternal?.MenuChanged(0,parentFormInternal.Menu);
    }

    //
    // Summary:
    //     Arranges the multiple-document interface (MDI) child forms within the MDI parent
    //     form.
    //
    // Parameters:
    //   value:
    //     One of the System.Windows.Forms.MdiLayout values that defines the layout of MDI
    //     child forms.
    public void LayoutMdi(MdiLayout value)
    {
      if(ctlClient!=null)
      {
        ctlClient.LayoutMdi(value);
      }
    }

    internal void MenuChanged(int change,Menu menu)
    {
      Form parentFormInternal = base.ParentFormInternal;
      if(parentFormInternal!=null&&this==parentFormInternal.ActiveMdiChildInternal)
      {
        parentFormInternal.MenuChanged(change,menu);
        return;
      }

      switch(change)
      {
        case 0:
        case 3:
        {
          if(ctlClient==null||!ctlClient.IsHandleCreated)
          {
            if(menu==Menu&&change==0)
            {
              UpdateMenuHandles();
            }

            break;
          }

          if(base.IsHandleCreated)
          {
            UpdateMenuHandles(null,forceRedraw: false);
          }

          Control.ControlCollection controls = ctlClient.Controls;
          int count = controls.Count;
          while(count-->0)
          {
            Control control = controls[count];
            if(control is Form&&control.Properties.ContainsObject(PropMergedMenu))
            {
              MainMenu mainMenu = control.Properties.GetObject(PropMergedMenu) as MainMenu;
              if(mainMenu!=null&&mainMenu.ownerForm==control)
              {
                mainMenu.Dispose();
              }

              control.Properties.SetObject(PropMergedMenu,null);
            }
          }

          UpdateMenuHandles();
          break;
        }
        case 1:
          if(menu==Menu||(ActiveMdiChildInternal!=null&&menu==ActiveMdiChildInternal.Menu))
          {
            UpdateMenuHandles();
          }

          break;
        case 2:
          if(ctlClient!=null&&ctlClient.IsHandleCreated)
          {
            UpdateMenuHandles();
          }

          break;
      }
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Form.Activated event.
    //
    // Parameters:
    //   e:
    //     An System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void OnActivated(EventArgs e)
    {
      ((EventHandler)base.Events[EVENT_ACTIVATED])?.Invoke(this,e);
    }

    internal override void OnAutoScaleModeChanged()
    {
      base.OnAutoScaleModeChanged();
      if(formStateEx[FormStateExSettingAutoScale]!=1)
      {
        AutoScale=false;
      }
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Control.BackgroundImageChanged event.
    //
    // Parameters:
    //   e:
    //     An System.EventArgs that contains the data.
    protected override void OnBackgroundImageChanged(EventArgs e)
    {
      base.OnBackgroundImageChanged(e);
      if(IsMdiContainer)
      {
        MdiClient.BackgroundImage=BackgroundImage;
        MdiClient.Invalidate();
      }
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Control.BackgroundImageLayoutChanged event.
    //
    // Parameters:
    //   e:
    //     An System.EventArgs that contains the event data.
    protected override void OnBackgroundImageLayoutChanged(EventArgs e)
    {
      base.OnBackgroundImageLayoutChanged(e);
      if(IsMdiContainer)
      {
        MdiClient.BackgroundImageLayout=BackgroundImageLayout;
        MdiClient.Invalidate();
      }
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Form.Closing event.
    //
    // Parameters:
    //   e:
    //     A System.ComponentModel.CancelEventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void OnClosing(CancelEventArgs e)
    {
      ((CancelEventHandler)base.Events[EVENT_CLOSING])?.Invoke(this,e);
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Form.Closed event.
    //
    // Parameters:
    //   e:
    //     The System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void OnClosed(EventArgs e)
    {
      ((EventHandler)base.Events[EVENT_CLOSED])?.Invoke(this,e);
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Form.FormClosing event.
    //
    // Parameters:
    //   e:
    //     A System.Windows.Forms.FormClosingEventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void OnFormClosing(FormClosingEventArgs e)
    {
      ((FormClosingEventHandler)base.Events[EVENT_FORMCLOSING])?.Invoke(this,e);
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Form.FormClosed event.
    //
    // Parameters:
    //   e:
    //     A System.Windows.Forms.FormClosedEventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void OnFormClosed(FormClosedEventArgs e)
    {
      Application.OpenFormsInternalRemove(this);
      ((FormClosedEventHandler)base.Events[EVENT_FORMCLOSED])?.Invoke(this,e);
    }

    //
    // Summary:
    //     Raises the CreateControl event.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected override void OnCreateControl()
    {
      CalledCreateControl=true;
      base.OnCreateControl();
      if(CalledMakeVisible&&!CalledOnLoad)
      {
        CalledOnLoad=true;
        OnLoad(EventArgs.Empty);
      }
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Form.Deactivate event.
    //
    // Parameters:
    //   e:
    //     The System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void OnDeactivate(EventArgs e)
    {
      ((EventHandler)base.Events[EVENT_DEACTIVATE])?.Invoke(this,e);
    }

    //
    // Parameters:
    //   e:
    //     An System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected override void OnEnabledChanged(EventArgs e)
    {
      base.OnEnabledChanged(e);
      if(!base.DesignMode&&base.Enabled&&Active)
      {
        Control control = base.ActiveControl;
        if(control==null)
        {
          SelectNextControlInternal(this,forward: true,tabStopOnly: true,nested: true,wrap: true);
        }
        else
        {
          FocusActiveControlInternal();
        }
      }
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Control.Enter event.
    //
    // Parameters:
    //   e:
    //     An System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected override void OnEnter(EventArgs e)
    {
      base.OnEnter(e);
      if(IsMdiChild)
      {
        UpdateFocusedControl();
      }
    }

    //
    // Parameters:
    //   e:
    //     An System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected override void OnFontChanged(EventArgs e)
    {
      if(base.DesignMode)
      {
        UpdateAutoScaleBaseSize();
      }

      base.OnFontChanged(e);
    }

    //
    // Parameters:
    //   e:
    //     An System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected override void OnHandleCreated(EventArgs e)
    {
      formStateEx[FormStateExUseMdiChildProc]=((IsMdiChild&&base.Visible) ? 1 : 0);
      base.OnHandleCreated(e);
      UpdateLayered();
    }

    //
    // Parameters:
    //   e:
    //     An System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected override void OnHandleDestroyed(EventArgs e)
    {
      base.OnHandleDestroyed(e);
      formStateEx[FormStateExUseMdiChildProc]=0;
      Application.OpenFormsInternalRemove(this);
      ResetSecurityTip(modalOnly: true);
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Form.HelpButtonClicked event.
    //
    // Parameters:
    //   e:
    //     A System.ComponentModel.CancelEventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void OnHelpButtonClicked(CancelEventArgs e)
    {
      ((CancelEventHandler)base.Events[EVENT_HELPBUTTONCLICKED])?.Invoke(this,e);
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
      if(AutoSize)
      {
        Size size = (minAutoSize=base.PreferredSize);
        Size size2 = ((AutoSizeMode==AutoSizeMode.GrowAndShrink) ? size : LayoutUtils.UnionSizes(size,Size));
        ((IArrangedElement)this)?.SetBounds(new Rectangle(base.Left,base.Top,size2.Width,size2.Height),BoundsSpecified.None);
      }

      base.OnLayout(levent);
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Form.Load event.
    //
    // Parameters:
    //   e:
    //     An System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void OnLoad(EventArgs e)
    {
      Application.OpenFormsInternalAdd(this);
      if(Application.UseWaitCursor)
      {
        base.UseWaitCursor=true;
      }

      if(formState[FormStateAutoScaling]==1&&!base.DesignMode)
      {
        formState[FormStateAutoScaling]=0;
        ApplyAutoScaling();
      }

      if(GetState(32))
      {
        switch(formState[FormStateStartPos])
        {
          case 4:
            CenterToParent();
            break;
          case 1:
            CenterToScreen();
            break;
        }
      }

      EventHandler eventHandler = (EventHandler)base.Events[EVENT_LOAD];
      if(eventHandler!=null)
      {
        _=Text;
        eventHandler(this,e);
        foreach(Control control in base.Controls)
        {
          control.Invalidate();
        }
      }

      if(base.IsHandleCreated)
      {
        BeginInvoke(new MethodInvoker(CallShownEvent));
      }
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Form.MaximizedBoundsChanged event.
    //
    // Parameters:
    //   e:
    //     The System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void OnMaximizedBoundsChanged(EventArgs e)
    {
      (base.Events[EVENT_MAXIMIZEDBOUNDSCHANGED] as EventHandler)?.Invoke(this,e);
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Form.MaximumSizeChanged event.
    //
    // Parameters:
    //   e:
    //     The System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void OnMaximumSizeChanged(EventArgs e)
    {
      (base.Events[EVENT_MAXIMUMSIZECHANGED] as EventHandler)?.Invoke(this,e);
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Form.MinimumSizeChanged event.
    //
    // Parameters:
    //   e:
    //     An System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void OnMinimumSizeChanged(EventArgs e)
    {
      (base.Events[EVENT_MINIMUMSIZECHANGED] as EventHandler)?.Invoke(this,e);
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Form.InputLanguageChanged event.
    //
    // Parameters:
    //   e:
    //     The System.Windows.Forms.InputLanguageChangedEventArgs that contains the event
    //     data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void OnInputLanguageChanged(InputLanguageChangedEventArgs e)
    {
      ((InputLanguageChangedEventHandler)base.Events[EVENT_INPUTLANGCHANGE])?.Invoke(this,e);
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Form.InputLanguageChanging event.
    //
    // Parameters:
    //   e:
    //     The System.Windows.Forms.InputLanguageChangingEventArgs that contains the event
    //     data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void OnInputLanguageChanging(InputLanguageChangingEventArgs e)
    {
      ((InputLanguageChangingEventHandler)base.Events[EVENT_INPUTLANGCHANGEREQUEST])?.Invoke(this,e);
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Control.VisibleChanged event.
    //
    // Parameters:
    //   e:
    //     The System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected override void OnVisibleChanged(EventArgs e)
    {
      UpdateRenderSizeGrip();
      MdiParentInternal?.UpdateMdiWindowListStrip();
      base.OnVisibleChanged(e);
      bool value = false;
      if(!base.IsHandleCreated||!base.Visible||AcceptButton==null||!UnsafeNativeMethods.SystemParametersInfo(95,0,ref value,0)||!value)
      {
        return;
      }

      Control control = AcceptButton as Control;
      NativeMethods.POINT pOINT = new NativeMethods.POINT(control.Left+control.Width/2,control.Top+control.Height/2);
      UnsafeNativeMethods.ClientToScreen(new HandleRef(this,base.Handle),pOINT);
      if(!control.IsWindowObscured)
      {
        IntSecurity.AdjustCursorPosition.Assert();
        try
        {
          Cursor.Position=new Point(pOINT.x,pOINT.y);
        }
        finally
        {
          CodeAccessPermission.RevertAssert();
        }
      }
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Form.MdiChildActivate event.
    //
    // Parameters:
    //   e:
    //     The System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void OnMdiChildActivate(EventArgs e)
    {
      UpdateMenuHandles();
      UpdateToolStrip();
      ((EventHandler)base.Events[EVENT_MDI_CHILD_ACTIVATE])?.Invoke(this,e);
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Form.MenuStart event.
    //
    // Parameters:
    //   e:
    //     The System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void OnMenuStart(EventArgs e)
    {
      ((SecurityToolTip)base.Properties.GetObject(PropSecurityTip))?.Pop(noLongerFirst: true);
      ((EventHandler)base.Events[EVENT_MENUSTART])?.Invoke(this,e);
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Form.MenuComplete event.
    //
    // Parameters:
    //   e:
    //     The System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void OnMenuComplete(EventArgs e)
    {
      ((EventHandler)base.Events[EVENT_MENUCOMPLETE])?.Invoke(this,e);
    }

    //
    // Parameters:
    //   e:
    //     A System.Windows.Forms.PaintEventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected override void OnPaint(PaintEventArgs e)
    {
      base.OnPaint(e);
      if(formState[FormStateRenderSizeGrip]!=0)
      {
        Size clientSize = ClientSize;
        if(Application.RenderWithVisualStyles)
        {
          if(sizeGripRenderer==null)
          {
            sizeGripRenderer=new VisualStyleRenderer(VisualStyleElement.Status.Gripper.Normal);
          }

          sizeGripRenderer.DrawBackground(e.Graphics,new Rectangle(clientSize.Width-16,clientSize.Height-16,16,16));
        }
        else
        {
          ControlPaint.DrawSizeGrip(e.Graphics,BackColor,clientSize.Width-16,clientSize.Height-16,16,16);
        }
      }

      if(IsMdiContainer)
      {
        e.Graphics.FillRectangle(SystemBrushes.AppWorkspace,base.ClientRectangle);
      }
    }

    //
    // Parameters:
    //   e:
    //     An System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected override void OnResize(EventArgs e)
    {
      base.OnResize(e);
      if(formState[FormStateRenderSizeGrip]!=0)
      {
        Invalidate();
      }
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Form.RightToLeftLayoutChanged event.
    //
    // Parameters:
    //   e:
    //     An System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void OnRightToLeftLayoutChanged(EventArgs e)
    {
      if(GetAnyDisposingInHierarchy())
      {
        return;
      }

      if(RightToLeft==RightToLeft.Yes)
      {
        RecreateHandle();
      }

      (base.Events[EVENT_RIGHTTOLEFTLAYOUTCHANGED] as EventHandler)?.Invoke(this,e);
      if(RightToLeft!=RightToLeft.Yes)
      {
        return;
      }

      foreach(Control control in base.Controls)
      {
        control.RecreateHandleCore();
      }
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Form.Shown event.
    //
    // Parameters:
    //   e:
    //     A System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void OnShown(EventArgs e)
    {
      ((EventHandler)base.Events[EVENT_SHOWN])?.Invoke(this,e);
    }

    //
    // Parameters:
    //   e:
    //     An System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected override void OnTextChanged(EventArgs e)
    {
      base.OnTextChanged(e);
      int num = ((Text.Length==0) ? 1 : 0);
      if(!ControlBox&&formState[FormStateIsTextEmpty]!=num)
      {
        RecreateHandle();
      }

      formState[FormStateIsTextEmpty]=num;
    }

    internal void PerformOnInputLanguageChanged(InputLanguageChangedEventArgs iplevent)
    {
      OnInputLanguageChanged(iplevent);
    }

    internal void PerformOnInputLanguageChanging(InputLanguageChangingEventArgs iplcevent)
    {
      OnInputLanguageChanging(iplcevent);
    }

    //
    // Summary:
    //     Processes a command key.
    //
    // Parameters:
    //   msg:
    //     A System.Windows.Forms.Message, passed by reference, that represents the Win32
    //     message to process.
    //
    //   keyData:
    //     One of the System.Windows.Forms.Keys values that represents the key to process.
    //
    // Returns:
    //     true if the keystroke was processed and consumed by the control; otherwise, false
    //     to allow further processing.
    [SecurityPermission(SecurityAction.LinkDemand,Flags = SecurityPermissionFlag.UnmanagedCode)]
    protected override bool ProcessCmdKey(ref Message msg,Keys keyData)
    {
      if(base.ProcessCmdKey(ref msg,keyData))
      {
        return true;
      }

      MainMenu mainMenu = (MainMenu)base.Properties.GetObject(PropCurMenu);
      if(mainMenu!=null&&mainMenu.ProcessCmdKey(ref msg,keyData))
      {
        return true;
      }

      bool result = false;
      NativeMethods.MSG msg2 = default(NativeMethods.MSG);
      msg2.message=msg.Msg;
      msg2.wParam=msg.WParam;
      msg2.lParam=msg.LParam;
      msg2.hwnd=msg.HWnd;
      if(ctlClient!=null&&ctlClient.Handle!=IntPtr.Zero&&UnsafeNativeMethods.TranslateMDISysAccel(ctlClient.Handle,ref msg2))
      {
        result=true;
      }

      msg.Msg=msg2.message;
      msg.WParam=msg2.wParam;
      msg.LParam=msg2.lParam;
      msg.HWnd=msg2.hwnd;
      return result;
    }

    //
    // Summary:
    //     Processes a dialog box key.
    //
    // Parameters:
    //   keyData:
    //     One of the System.Windows.Forms.Keys values that represents the key to process.
    //
    // Returns:
    //     true if the keystroke was processed and consumed by the control; otherwise, false
    //     to allow further processing.
    [UIPermission(SecurityAction.LinkDemand,Window = UIPermissionWindow.AllWindows)]
    protected override bool ProcessDialogKey(Keys keyData)
    {
      if((keyData&(Keys.Control|Keys.Alt))==0)
      {
        switch(keyData&Keys.KeyCode)
        {
          case Keys.Return:
          {
            IButtonControl buttonControl = (IButtonControl)base.Properties.GetObject(PropDefaultButton);
            if(buttonControl!=null)
            {
              if(buttonControl is Control)
              {
                buttonControl.PerformClick();
              }

              return true;
            }

            break;
          }
          case Keys.Escape:
          {
            IButtonControl buttonControl = (IButtonControl)base.Properties.GetObject(PropCancelButton);
            if(buttonControl!=null)
            {
              buttonControl.PerformClick();
              return true;
            }

            break;
          }
        }
      }

      return base.ProcessDialogKey(keyData);
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [UIPermission(SecurityAction.LinkDemand,Window = UIPermissionWindow.AllWindows)]
    protected override bool ProcessDialogChar(char charCode)
    {
      if(IsMdiChild&&charCode!=' ')
      {
        if(ProcessMnemonic(charCode))
        {
          return true;
        }

        formStateEx[FormStateExMnemonicProcessed]=1;
        try
        {
          return base.ProcessDialogChar(charCode);
        }
        finally
        {
          formStateEx[FormStateExMnemonicProcessed]=0;
        }
      }

      return base.ProcessDialogChar(charCode);
    }

    //
    // Parameters:
    //   m:
    //     A System.Windows.Forms.Message, passed by reference, that represents the window
    //     message to process.
    //
    // Returns:
    //     true if the message was processed by the control; otherwise, false.
    [SecurityPermission(SecurityAction.LinkDemand,Flags = SecurityPermissionFlag.UnmanagedCode)]
    protected override bool ProcessKeyPreview(ref Message m)
    {
      if(formState[FormStateKeyPreview]!=0&&ProcessKeyEventArgs(ref m))
      {
        return true;
      }

      return base.ProcessKeyPreview(ref m);
    }

    //
    // Parameters:
    //   forward:
    //     true to cycle forward through the controls in the System.Windows.Forms.ContainerControl;
    //     otherwise, false.
    //
    // Returns:
    //     true if a control is selected; otherwise, false.
    [UIPermission(SecurityAction.LinkDemand,Window = UIPermissionWindow.AllWindows)]
    protected override bool ProcessTabKey(bool forward)
    {
      if(SelectNextControl(base.ActiveControl,forward,tabStopOnly: true,nested: true,wrap: true))
      {
        return true;
      }

      if((IsMdiChild||base.ParentFormInternal==null)&&SelectNextControl(null,forward,tabStopOnly: true,nested: true,wrap: false))
      {
        return true;
      }

      return false;
    }

    internal void RaiseFormClosedOnAppExit()
    {
      if(!Modal)
      {
        int integer = base.Properties.GetInteger(PropOwnedFormsCount);
        if(integer>0)
        {
          Form[] ownedForms = OwnedForms;
          FormClosedEventArgs e = new FormClosedEventArgs(CloseReason.FormOwnerClosing);
          for(int num = integer-1;num>=0;num--)
          {
            if(ownedForms[num]!=null&&!Application.OpenFormsInternal.Contains(ownedForms[num]))
            {
              ownedForms[num].OnFormClosed(e);
            }
          }
        }
      }

      OnFormClosed(new FormClosedEventArgs(CloseReason.ApplicationExitCall));
    }

    internal bool RaiseFormClosingOnAppExit()
    {
      FormClosingEventArgs formClosingEventArgs = new FormClosingEventArgs(CloseReason.ApplicationExitCall,cancel: false);
      if(!Modal)
      {
        int integer = base.Properties.GetInteger(PropOwnedFormsCount);
        if(integer>0)
        {
          Form[] ownedForms = OwnedForms;
          FormClosingEventArgs formClosingEventArgs2 = new FormClosingEventArgs(CloseReason.FormOwnerClosing,cancel: false);
          for(int num = integer-1;num>=0;num--)
          {
            if(ownedForms[num]!=null&&!Application.OpenFormsInternal.Contains(ownedForms[num]))
            {
              ownedForms[num].OnFormClosing(formClosingEventArgs2);
              if(formClosingEventArgs2.Cancel)
              {
                formClosingEventArgs.Cancel=true;
                break;
              }
            }
          }
        }
      }

      OnFormClosing(formClosingEventArgs);
      return formClosingEventArgs.Cancel;
    }

    internal override void RecreateHandleCore()
    {
      NativeMethods.WINDOWPLACEMENT placement = default(NativeMethods.WINDOWPLACEMENT);
      FormStartPosition formStartPosition = FormStartPosition.Manual;
      if(!IsMdiChild&&(WindowState==FormWindowState.Minimized||WindowState==FormWindowState.Maximized))
      {
        placement.length=Marshal.SizeOf(typeof(NativeMethods.WINDOWPLACEMENT));
        UnsafeNativeMethods.GetWindowPlacement(new HandleRef(this,base.Handle),ref placement);
      }

      if(StartPosition!=0)
      {
        formStartPosition=StartPosition;
        StartPosition=FormStartPosition.Manual;
      }

      EnumThreadWindowsCallback enumThreadWindowsCallback = null;
      SafeNativeMethods.EnumThreadWindowsCallback enumThreadWindowsCallback2 = null;
      if(base.IsHandleCreated)
      {
        enumThreadWindowsCallback=new EnumThreadWindowsCallback();
        if(enumThreadWindowsCallback!=null)
        {
          enumThreadWindowsCallback2=enumThreadWindowsCallback.Callback;
          UnsafeNativeMethods.EnumThreadWindows(SafeNativeMethods.GetCurrentThreadId(),enumThreadWindowsCallback2.Invoke,new HandleRef(this,base.Handle));
          enumThreadWindowsCallback.ResetOwners();
        }
      }

      base.RecreateHandleCore();
      enumThreadWindowsCallback?.SetOwners(new HandleRef(this,base.Handle));
      if(formStartPosition!=0)
      {
        StartPosition=formStartPosition;
      }

      if(placement.length>0)
      {
        UnsafeNativeMethods.SetWindowPlacement(new HandleRef(this,base.Handle),ref placement);
      }

      if(enumThreadWindowsCallback2!=null)
      {
        GC.KeepAlive(enumThreadWindowsCallback2);
      }
    }

    //
    // Summary:
    //     Removes an owned form from this form.
    //
    // Parameters:
    //   ownedForm:
    //     A System.Windows.Forms.Form representing the form to remove from the list of
    //     owned forms for this form.
    public void RemoveOwnedForm(Form ownedForm)
    {
      if(ownedForm==null)
      {
        return;
      }

      if(ownedForm.OwnerInternal!=null)
      {
        ownedForm.Owner=null;
        return;
      }

      Form[] array = (Form[])base.Properties.GetObject(PropOwnedForms);
      int num = base.Properties.GetInteger(PropOwnedFormsCount);
      if(array==null)
      {
        return;
      }

      for(int i = 0;i<num;i++)
      {
        if(ownedForm.Equals(array[i]))
        {
          array[i]=null;
          if(i+1<num)
          {
            Array.Copy(array,i+1,array,i,num-i-1);
            array[num-1]=null;
          }

          num--;
        }
      }

      base.Properties.SetInteger(PropOwnedFormsCount,num);
    }

    private void ResetIcon()
    {
      icon=null;
      if(smallIcon!=null)
      {
        smallIcon.Dispose();
        smallIcon=null;
      }

      formState[FormStateIconSet]=0;
      UpdateWindowIcon(redrawFrame: true);
    }

    private void ResetSecurityTip(bool modalOnly)
    {
      SecurityToolTip securityToolTip = (SecurityToolTip)base.Properties.GetObject(PropSecurityTip);
      if(securityToolTip!=null&&((modalOnly&&securityToolTip.Modal)||!modalOnly))
      {
        securityToolTip.Dispose();
        securityToolTip=null;
        base.Properties.SetObject(PropSecurityTip,null);
      }
    }

    private void ResetTransparencyKey()
    {
      TransparencyKey=Color.Empty;
    }

    private void ResumeLayoutFromMinimize()
    {
      if(formState[FormStateWindowState]==1)
      {
        ResumeLayout();
      }
    }

    private void RestoreWindowBoundsIfNecessary()
    {
      if(WindowState==FormWindowState.Normal)
      {
        Size size = restoredWindowBounds.Size;
        if((restoredWindowBoundsSpecified&BoundsSpecified.Size)!=0)
        {
          size=SizeFromClientSize(size.Width,size.Height);
        }

        SetBounds(restoredWindowBounds.X,restoredWindowBounds.Y,(formStateEx[FormStateExWindowBoundsWidthIsClientSize]==1) ? size.Width : restoredWindowBounds.Width,(formStateEx[FormStateExWindowBoundsHeightIsClientSize]==1) ? size.Height : restoredWindowBounds.Height,restoredWindowBoundsSpecified);
        restoredWindowBoundsSpecified=BoundsSpecified.None;
        restoredWindowBounds=new Rectangle(-1,-1,-1,-1);
        formStateEx[FormStateExWindowBoundsHeightIsClientSize]=0;
        formStateEx[FormStateExWindowBoundsWidthIsClientSize]=0;
      }
    }

    private void RestrictedProcessNcActivate()
    {
      if(base.IsDisposed||base.Disposing)
      {
        return;
      }

      SecurityToolTip securityToolTip = (SecurityToolTip)base.Properties.GetObject(PropSecurityTip);
      if(securityToolTip==null)
      {
        if(base.IsHandleCreated&&UnsafeNativeMethods.GetForegroundWindow()==base.Handle)
        {
          securityToolTip=new SecurityToolTip(this);
          base.Properties.SetObject(PropSecurityTip,securityToolTip);
        }
      }
      else if(!base.IsHandleCreated||UnsafeNativeMethods.GetForegroundWindow()!=base.Handle)
      {
        securityToolTip.Pop(noLongerFirst: false);
      }
      else
      {
        securityToolTip.Show();
      }
    }

    private void ResumeUpdateMenuHandles()
    {
      int num = formStateEx[FormStateExUpdateMenuHandlesSuspendCount];
      if(num<=0)
      {
        throw new InvalidOperationException(SR.GetString("TooManyResumeUpdateMenuHandles"));
      }

      num=(formStateEx[FormStateExUpdateMenuHandlesSuspendCount]=num-1);
      if(num==0&&formStateEx[FormStateExUpdateMenuHandlesDeferred]!=0)
      {
        UpdateMenuHandles();
      }
    }

    //
    // Summary:
    //     Selects this form, and optionally selects the next or previous control.
    //
    // Parameters:
    //   directed:
    //     If set to true that the active control is changed
    //
    //   forward:
    //     If directed is true, then this controls the direction in which focus is moved.
    //     If this is true, then the next control is selected; otherwise, the previous control
    //     is selected.
    protected override void Select(bool directed,bool forward)
    {
      IntSecurity.ModifyFocus.Demand();
      SelectInternal(directed,forward);
    }

    private void SelectInternal(bool directed,bool forward)
    {
      IntSecurity.ModifyFocus.Assert();
      if(directed)
      {
        SelectNextControl(null,forward,tabStopOnly: true,nested: true,wrap: false);
      }

      if(TopLevel)
      {
        UnsafeNativeMethods.SetActiveWindow(new HandleRef(this,base.Handle));
        return;
      }

      if(IsMdiChild)
      {
        UnsafeNativeMethods.SetActiveWindow(new HandleRef(MdiParentInternal,MdiParentInternal.Handle));
        MdiParentInternal.MdiClient.SendMessage(546,base.Handle,0);
        return;
      }

      Form parentFormInternal = base.ParentFormInternal;
      if(parentFormInternal!=null)
      {
        parentFormInternal.ActiveControl=this;
      }
    }

    //
    // Summary:
    //     Performs scaling of the form.
    //
    // Parameters:
    //   x:
    //     Percentage to scale the form horizontally
    //
    //   y:
    //     Percentage to scale the form vertically
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected override void ScaleCore(float x,float y)
    {
      SuspendLayout();
      try
      {
        if(WindowState==FormWindowState.Normal)
        {
          Size clientSize = ClientSize;
          Size minimumSize = MinimumSize;
          Size maximumSize = MaximumSize;
          if(!MinimumSize.IsEmpty)
          {
            MinimumSize=ScaleSize(minimumSize,x,y);
          }

          if(!MaximumSize.IsEmpty)
          {
            MaximumSize=ScaleSize(maximumSize,x,y);
          }

          ClientSize=ScaleSize(clientSize,x,y);
        }

        ScaleDockPadding(x,y);
        foreach(Control control in base.Controls)
        {
          control?.Scale(x,y);
        }
      }
      finally
      {
        ResumeLayout();
      }
    }

    //
    // Parameters:
    //   bounds:
    //     A System.Drawing.Rectangle that specifies the area for which to retrieve the
    //     display bounds.
    //
    //   factor:
    //     The height and width of the control's bounds.
    //
    //   specified:
    //     One of the values of System.Windows.Forms.BoundsSpecified that specifies the
    //     bounds of the control to use when defining its size and position.
    //
    // Returns:
    //     A System.Drawing.Rectangle representing the bounds within which the control is
    //     scaled.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected override Rectangle GetScaledBounds(Rectangle bounds,SizeF factor,BoundsSpecified specified)
    {
      if(WindowState!=0)
      {
        bounds=RestoreBounds;
      }

      return base.GetScaledBounds(bounds,factor,specified);
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected override void ScaleControl(SizeF factor,BoundsSpecified specified)
    {
      formStateEx[FormStateExInScale]=1;
      try
      {
        if(MdiParentInternal!=null)
        {
          specified&=~BoundsSpecified.Location;
        }

        base.ScaleControl(factor,specified);
      }
      finally
      {
        formStateEx[FormStateExInScale]=0;
      }
    }

    //
    // Parameters:
    //   x:
    //     The x-coordinate.
    //
    //   y:
    //     The y-coordinate.
    //
    //   width:
    //     The bounds width.
    //
    //   height:
    //     The bounds height.
    //
    //   specified:
    //     A value from the BoundsSpecified enumeration.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected override void SetBoundsCore(int x,int y,int width,int height,BoundsSpecified specified)
    {
      if(WindowState!=0)
      {
        if(x!=-1||y!=-1)
        {
          restoredWindowBoundsSpecified|=specified&BoundsSpecified.Location;
        }

        restoredWindowBoundsSpecified|=specified&BoundsSpecified.Size;
        if((specified&BoundsSpecified.X)!=0)
        {
          restoredWindowBounds.X=x;
        }

        if((specified&BoundsSpecified.Y)!=0)
        {
          restoredWindowBounds.Y=y;
        }

        if((specified&BoundsSpecified.Width)!=0)
        {
          restoredWindowBounds.Width=width;
          formStateEx[FormStateExWindowBoundsWidthIsClientSize]=0;
        }

        if((specified&BoundsSpecified.Height)!=0)
        {
          restoredWindowBounds.Height=height;
          formStateEx[FormStateExWindowBoundsHeightIsClientSize]=0;
        }
      }

      if((specified&BoundsSpecified.X)!=0)
      {
        restoreBounds.X=x;
      }

      if((specified&BoundsSpecified.Y)!=0)
      {
        restoreBounds.Y=y;
      }

      if((specified&BoundsSpecified.Width)!=0||restoreBounds.Width==-1)
      {
        restoreBounds.Width=width;
      }

      if((specified&BoundsSpecified.Height)!=0||restoreBounds.Height==-1)
      {
        restoreBounds.Height=height;
      }

      if(WindowState==FormWindowState.Normal&&(base.Height!=height||base.Width!=width))
      {
        Size maxWindowTrackSize = SystemInformation.MaxWindowTrackSize;
        if(height>maxWindowTrackSize.Height)
        {
          height=maxWindowTrackSize.Height;
        }

        if(width>maxWindowTrackSize.Width)
        {
          width=maxWindowTrackSize.Width;
        }
      }

      FormBorderStyle formBorderStyle = FormBorderStyle;
      if(formBorderStyle!=0&&formBorderStyle!=FormBorderStyle.FixedToolWindow&&formBorderStyle!=FormBorderStyle.SizableToolWindow&&ParentInternal==null)
      {
        Size minWindowTrackSize = SystemInformation.MinWindowTrackSize;
        if(height<minWindowTrackSize.Height)
        {
          height=minWindowTrackSize.Height;
        }

        if(width<minWindowTrackSize.Width)
        {
          width=minWindowTrackSize.Width;
        }
      }

      if(IsRestrictedWindow)
      {
        Rectangle rectangle = ApplyBoundsConstraints(x,y,width,height);
        if(rectangle!=new Rectangle(x,y,width,height))
        {
          base.SetBoundsCore(rectangle.X,rectangle.Y,rectangle.Width,rectangle.Height,BoundsSpecified.All);
          return;
        }
      }

      base.SetBoundsCore(x,y,width,height,specified);
    }

    internal override Rectangle ApplyBoundsConstraints(int suggestedX,int suggestedY,int proposedWidth,int proposedHeight)
    {
      Rectangle rectangle = base.ApplyBoundsConstraints(suggestedX,suggestedY,proposedWidth,proposedHeight);
      if(IsRestrictedWindow)
      {
        Screen[] allScreens = Screen.AllScreens;
        bool flag = false;
        bool flag2 = false;
        bool flag3 = false;
        bool flag4 = false;
        for(int i = 0;i<allScreens.Length;i++)
        {
          Rectangle workingArea = allScreens[i].WorkingArea;
          if(workingArea.Contains(suggestedX,suggestedY))
          {
            flag=true;
          }

          if(workingArea.Contains(suggestedX+proposedWidth,suggestedY))
          {
            flag2=true;
          }

          if(workingArea.Contains(suggestedX,suggestedY+proposedHeight))
          {
            flag3=true;
          }

          if(workingArea.Contains(suggestedX+proposedWidth,suggestedY+proposedHeight))
          {
            flag4=true;
          }
        }

        if(!flag||!flag2||!flag3||!flag4)
        {
          if(formStateEx[FormStateExInScale]==1)
          {
            rectangle=WindowsFormsUtils.ConstrainToScreenWorkingAreaBounds(rectangle);
          }
          else
          {
            rectangle.X=base.Left;
            rectangle.Y=base.Top;
            rectangle.Width=base.Width;
            rectangle.Height=base.Height;
          }
        }
      }

      return rectangle;
    }

    private void SetDefaultButton(IButtonControl button)
    {
      IButtonControl buttonControl = (IButtonControl)base.Properties.GetObject(PropDefaultButton);
      if(buttonControl!=button)
      {
        buttonControl?.NotifyDefault(value: false);
        base.Properties.SetObject(PropDefaultButton,button);
        button?.NotifyDefault(value: true);
      }
    }

    //
    // Summary:
    //     Sets the client size of the form. This will adjust the bounds of the form to
    //     make the client size the requested size.
    //
    // Parameters:
    //   x:
    //     Requested width of the client region.
    //
    //   y:
    //     Requested height of the client region.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected override void SetClientSizeCore(int x,int y)
    {
      bool hScroll = base.HScroll;
      bool vScroll = base.VScroll;
      base.SetClientSizeCore(x,y);
      if(base.IsHandleCreated)
      {
        if(base.VScroll!=vScroll&&base.VScroll)
        {
          x+=SystemInformation.VerticalScrollBarWidth;
        }

        if(base.HScroll!=hScroll&&base.HScroll)
        {
          y+=SystemInformation.HorizontalScrollBarHeight;
        }

        if(x!=ClientSize.Width||y!=ClientSize.Height)
        {
          base.SetClientSizeCore(x,y);
        }
      }

      formState[FormStateSetClientSize]=1;
    }

    //
    // Summary:
    //     Sets the bounds of the form in desktop coordinates.
    //
    // Parameters:
    //   x:
    //     The x-coordinate of the form's location.
    //
    //   y:
    //     The y-coordinate of the form's location.
    //
    //   width:
    //     The width of the form.
    //
    //   height:
    //     The height of the form.
    public void SetDesktopBounds(int x,int y,int width,int height)
    {
      Rectangle workingArea = SystemInformation.WorkingArea;
      SetBounds(x+workingArea.X,y+workingArea.Y,width,height,BoundsSpecified.All);
    }

    //
    // Summary:
    //     Sets the location of the form in desktop coordinates.
    //
    // Parameters:
    //   x:
    //     The x-coordinate of the form's location.
    //
    //   y:
    //     The y-coordinate of the form's location.
    public void SetDesktopLocation(int x,int y)
    {
      Rectangle workingArea = SystemInformation.WorkingArea;
      Location=new Point(workingArea.X+x,workingArea.Y+y);
    }

    //
    // Summary:
    //     Shows the form with the specified owner to the user.
    //
    // Parameters:
    //   owner:
    //     Any object that implements System.Windows.Forms.IWin32Window and represents the
    //     top-level window that will own this form.
    //
    // Exceptions:
    //   T:System.InvalidOperationException:
    //     The form being shown is already visible.-or- The form specified in the owner
    //     parameter is the same as the form being shown.-or- The form being shown is disabled.-or-
    //     The form being shown is not a top-level window.-or- The form being shown as a
    //     dialog box is already a modal form.-or-The current process is not running in
    //     user interactive mode (for more information, see System.Windows.Forms.SystemInformation.UserInteractive).
    public void Show(IWin32Window owner)
    {
      if(owner==this)
      {
        throw new InvalidOperationException(SR.GetString("OwnsSelfOrOwner","Show"));
      }

      if(base.Visible)
      {
        throw new InvalidOperationException(SR.GetString("ShowDialogOnVisible","Show"));
      }

      if(!base.Enabled)
      {
        throw new InvalidOperationException(SR.GetString("ShowDialogOnDisabled","Show"));
      }

      if(!TopLevel)
      {
        throw new InvalidOperationException(SR.GetString("ShowDialogOnNonTopLevel","Show"));
      }

      if(!SystemInformation.UserInteractive)
      {
        throw new InvalidOperationException(SR.GetString("CantShowModalOnNonInteractive"));
      }

      if(owner!=null&&((int)UnsafeNativeMethods.GetWindowLong(new HandleRef(owner,Control.GetSafeHandle(owner)),-20)&8)==0&&owner is Control)
      {
        owner=((Control)owner).TopLevelControlInternal;
      }

      IntPtr activeWindow = UnsafeNativeMethods.GetActiveWindow();
      IntPtr intPtr = ((owner==null) ? activeWindow : Control.GetSafeHandle(owner));
      _=IntPtr.Zero;
      base.Properties.SetObject(PropDialogOwner,owner);
      Form ownerInternal = OwnerInternal;
      if(owner is Form&&owner!=ownerInternal)
      {
        Owner=(Form)owner;
      }

      if(intPtr!=IntPtr.Zero&&intPtr!=base.Handle)
      {
        if(UnsafeNativeMethods.GetWindowLong(new HandleRef(owner,intPtr),-8)==base.Handle)
        {
          throw new ArgumentException(SR.GetString("OwnsSelfOrOwner","show"),"owner");
        }

        UnsafeNativeMethods.GetWindowLong(new HandleRef(this,base.Handle),-8);
        UnsafeNativeMethods.SetWindowLong(new HandleRef(this,base.Handle),-8,new HandleRef(owner,intPtr));
      }

      base.Visible=true;
    }

    //
    // Summary:
    //     Shows the form as a modal dialog box.
    //
    // Returns:
    //     One of the System.Windows.Forms.DialogResult values.
    //
    // Exceptions:
    //   T:System.InvalidOperationException:
    //     The form being shown is already visible.-or- The form being shown is disabled.-or-
    //     The form being shown is not a top-level window.-or- The form being shown as a
    //     dialog box is already a modal form.-or-The current process is not running in
    //     user interactive mode (for more information, see System.Windows.Forms.SystemInformation.UserInteractive).
    public DialogResult ShowDialog()
    {
      return ShowDialog(null);
    }

    //
    // Summary:
    //     Shows the form as a modal dialog box with the specified owner.
    //
    // Parameters:
    //   owner:
    //     Any object that implements System.Windows.Forms.IWin32Window that represents
    //     the top-level window that will own the modal dialog box.
    //
    // Returns:
    //     One of the System.Windows.Forms.DialogResult values.
    //
    // Exceptions:
    //   T:System.ArgumentException:
    //     The form specified in the owner parameter is the same as the form being shown.
    //
    //   T:System.InvalidOperationException:
    //     The form being shown is already visible.-or- The form being shown is disabled.-or-
    //     The form being shown is not a top-level window.-or- The form being shown as a
    //     dialog box is already a modal form.-or-The current process is not running in
    //     user interactive mode (for more information, see System.Windows.Forms.SystemInformation.UserInteractive).
    public DialogResult ShowDialog(IWin32Window owner)
    {
      if(owner==this)
      {
        throw new ArgumentException(SR.GetString("OwnsSelfOrOwner","showDialog"),"owner");
      }

      if(base.Visible)
      {
        throw new InvalidOperationException(SR.GetString("ShowDialogOnVisible","showDialog"));
      }

      if(!base.Enabled)
      {
        throw new InvalidOperationException(SR.GetString("ShowDialogOnDisabled","showDialog"));
      }

      if(!TopLevel)
      {
        throw new InvalidOperationException(SR.GetString("ShowDialogOnNonTopLevel","showDialog"));
      }

      if(Modal)
      {
        throw new InvalidOperationException(SR.GetString("ShowDialogOnModal","showDialog"));
      }

      if(!SystemInformation.UserInteractive)
      {
        throw new InvalidOperationException(SR.GetString("CantShowModalOnNonInteractive"));
      }

      if(owner!=null&&((int)UnsafeNativeMethods.GetWindowLong(new HandleRef(owner,Control.GetSafeHandle(owner)),-20)&8)==0&&owner is Control)
      {
        owner=((Control)owner).TopLevelControlInternal;
      }

      CalledOnLoad=false;
      CalledMakeVisible=false;
      CloseReason=CloseReason.None;
      IntPtr capture = UnsafeNativeMethods.GetCapture();
      if(capture!=IntPtr.Zero)
      {
        UnsafeNativeMethods.SendMessage(new HandleRef(null,capture),31,IntPtr.Zero,IntPtr.Zero);
        SafeNativeMethods.ReleaseCapture();
      }

      IntPtr intPtr = UnsafeNativeMethods.GetActiveWindow();
      IntPtr intPtr2 = ((owner==null) ? intPtr : Control.GetSafeHandle(owner));
      _=IntPtr.Zero;
      base.Properties.SetObject(PropDialogOwner,owner);
      Form ownerInternal = OwnerInternal;
      if(owner is Form&&owner!=ownerInternal)
      {
        Owner=(Form)owner;
      }

      try
      {
        SetState(32,value: true);
        dialogResult=DialogResult.None;
        CreateControl();
        if(intPtr2!=IntPtr.Zero&&intPtr2!=base.Handle)
        {
          if(UnsafeNativeMethods.GetWindowLong(new HandleRef(owner,intPtr2),-8)==base.Handle)
          {
            throw new ArgumentException(SR.GetString("OwnsSelfOrOwner","showDialog"),"owner");
          }

          UnsafeNativeMethods.GetWindowLong(new HandleRef(this,base.Handle),-8);
          UnsafeNativeMethods.SetWindowLong(new HandleRef(this,base.Handle),-8,new HandleRef(owner,intPtr2));
        }

        try
        {
          if(dialogResult==DialogResult.None)
          {
            Application.RunDialog(this);
          }
        }
        finally
        {
          if(!UnsafeNativeMethods.IsWindow(new HandleRef(null,intPtr)))
          {
            intPtr=intPtr2;
          }

          if(UnsafeNativeMethods.IsWindow(new HandleRef(null,intPtr))&&SafeNativeMethods.IsWindowVisible(new HandleRef(null,intPtr)))
          {
            UnsafeNativeMethods.SetActiveWindow(new HandleRef(null,intPtr));
          }
          else if(UnsafeNativeMethods.IsWindow(new HandleRef(null,intPtr2))&&SafeNativeMethods.IsWindowVisible(new HandleRef(null,intPtr2)))
          {
            UnsafeNativeMethods.SetActiveWindow(new HandleRef(null,intPtr2));
          }

          SetVisibleCore(value: false);
          if(base.IsHandleCreated)
          {
            if(OwnerInternal!=null&&OwnerInternal.IsMdiContainer)
            {
              OwnerInternal.Invalidate(invalidateChildren: true);
              OwnerInternal.Update();
            }

            DestroyHandle();
          }

          SetState(32,value: false);
        }
      }
      finally
      {
        Owner=ownerInternal;
        base.Properties.SetObject(PropDialogOwner,null);
      }

      return DialogResult;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal virtual bool ShouldSerializeAutoScaleBaseSize()
    {
      return formState[FormStateAutoScaling]!=0;
    }

    private bool ShouldSerializeClientSize()
    {
      return true;
    }

    private bool ShouldSerializeIcon()
    {
      return formState[FormStateIconSet]==1;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    private bool ShouldSerializeLocation()
    {
      if(base.Left==0)
      {
        return base.Top!=0;
      }

      return true;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal override bool ShouldSerializeSize()
    {
      return false;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal bool ShouldSerializeTransparencyKey()
    {
      return !TransparencyKey.Equals(Color.Empty);
    }

    private void SuspendLayoutForMinimize()
    {
      if(formState[FormStateWindowState]!=1)
      {
        SuspendLayout();
      }
    }

    private void SuspendUpdateMenuHandles()
    {
      int num = formStateEx[FormStateExUpdateMenuHandlesSuspendCount];
      num=(formStateEx[FormStateExUpdateMenuHandlesSuspendCount]=num+1);
    }

    //
    // Summary:
    //     Gets a string representing the current instance of the form.
    //
    // Returns:
    //     A string consisting of the fully qualified name of the form object's class, with
    //     the System.Windows.Forms.Form.Text property of the form appended to the end.
    //     For example, if the form is derived from the class MyForm in the MyNamespace
    //     namespace, and the System.Windows.Forms.Form.Text property is set to Hello, World,
    //     this method will return MyNamespace.MyForm, Text: Hello, World.
    public override string ToString()
    {
      string text = base.ToString();
      return text+", Text: "+Text;
    }

    private void UpdateAutoScaleBaseSize()
    {
      autoScaleBaseSize=Size.Empty;
    }

    private void UpdateRenderSizeGrip()
    {
      int num = formState[FormStateRenderSizeGrip];
      switch(FormBorderStyle)
      {
        case FormBorderStyle.None:
        case FormBorderStyle.FixedSingle:
        case FormBorderStyle.Fixed3D:
        case FormBorderStyle.FixedDialog:
        case FormBorderStyle.FixedToolWindow:
          formState[FormStateRenderSizeGrip]=0;
          break;
        case FormBorderStyle.Sizable:
        case FormBorderStyle.SizableToolWindow:
          switch(SizeGripStyle)
          {
            case SizeGripStyle.Show:
              formState[FormStateRenderSizeGrip]=1;
              break;
            case SizeGripStyle.Hide:
              formState[FormStateRenderSizeGrip]=0;
              break;
            case SizeGripStyle.Auto:
              if(GetState(32))
              {
                formState[FormStateRenderSizeGrip]=1;
              }
              else
              {
                formState[FormStateRenderSizeGrip]=0;
              }

              break;
          }

          break;
      }

      if(formState[FormStateRenderSizeGrip]!=num)
      {
        Invalidate();
      }
    }

    //
    // Summary:
    //     Updates which button is the default button.
    protected override void UpdateDefaultButton()
    {
      ContainerControl containerControl = this;
      while(containerControl.ActiveControl is ContainerControl)
      {
        containerControl=containerControl.ActiveControl as ContainerControl;
        if(containerControl is Form)
        {
          containerControl=this;
          break;
        }
      }

      if(containerControl.ActiveControl is IButtonControl)
      {
        SetDefaultButton((IButtonControl)containerControl.ActiveControl);
      }
      else
      {
        SetDefaultButton(AcceptButton);
      }
    }

    private void UpdateHandleWithOwner()
    {
      if(base.IsHandleCreated&&TopLevel)
      {
        HandleRef dwNewLong = NativeMethods.NullHandleRef;
        Form form = (Form)base.Properties.GetObject(PropOwner);
        if(form!=null)
        {
          dwNewLong=new HandleRef(form,form.Handle);
        }
        else if(!ShowInTaskbar)
        {
          dwNewLong=TaskbarOwner;
        }

        UnsafeNativeMethods.SetWindowLong(new HandleRef(this,base.Handle),-8,dwNewLong);
      }
    }

    private void UpdateLayered()
    {
      if(formState[FormStateLayered]!=0&&base.IsHandleCreated&&TopLevel&&OSFeature.Feature.IsPresent(OSFeature.LayeredWindows))
      {
        Color transparencyKey = TransparencyKey;
        if(!(transparencyKey.IsEmpty ? UnsafeNativeMethods.SetLayeredWindowAttributes(new HandleRef(this,base.Handle),0,OpacityAsByte,2) : ((OpacityAsByte!=byte.MaxValue) ? UnsafeNativeMethods.SetLayeredWindowAttributes(new HandleRef(this,base.Handle),ColorTranslator.ToWin32(transparencyKey),OpacityAsByte,3) : UnsafeNativeMethods.SetLayeredWindowAttributes(new HandleRef(this,base.Handle),ColorTranslator.ToWin32(transparencyKey),0,1))))
        {
          throw new Win32Exception();
        }
      }
    }

    private void UpdateMenuHandles()
    {
      if(base.Properties.GetObject(PropCurMenu)!=null)
      {
        base.Properties.SetObject(PropCurMenu,null);
      }

      if(!base.IsHandleCreated)
      {
        return;
      }

      if(!TopLevel)
      {
        UpdateMenuHandles(null,forceRedraw: true);
        return;
      }

      Form activeMdiChildInternal = ActiveMdiChildInternal;
      if(activeMdiChildInternal!=null)
      {
        UpdateMenuHandles(activeMdiChildInternal.MergedMenuPrivate,forceRedraw: true);
      }
      else
      {
        UpdateMenuHandles(Menu,forceRedraw: true);
      }
    }

    private void UpdateMenuHandles(MainMenu menu,bool forceRedraw)
    {
      int num = formStateEx[FormStateExUpdateMenuHandlesSuspendCount];
      if(num>0&&menu!=null)
      {
        formStateEx[FormStateExUpdateMenuHandlesDeferred]=1;
        return;
      }

      if(menu!=null)
      {
        menu.form=this;
      }

      if(menu!=null||base.Properties.ContainsObject(PropCurMenu))
      {
        base.Properties.SetObject(PropCurMenu,menu);
      }

      if(ctlClient==null||!ctlClient.IsHandleCreated)
      {
        if(menu!=null)
        {
          UnsafeNativeMethods.SetMenu(new HandleRef(this,base.Handle),new HandleRef(menu,menu.Handle));
        }
        else
        {
          UnsafeNativeMethods.SetMenu(new HandleRef(this,base.Handle),NativeMethods.NullHandleRef);
        }
      }
      else
      {
        MenuStrip mainMenuStrip = MainMenuStrip;
        if(mainMenuStrip==null||menu!=null)
        {
          MainMenu mainMenu = (MainMenu)base.Properties.GetObject(PropDummyMenu);
          if(mainMenu==null)
          {
            mainMenu=new MainMenu();
            mainMenu.ownerForm=this;
            base.Properties.SetObject(PropDummyMenu,mainMenu);
          }

          UnsafeNativeMethods.SendMessage(new HandleRef(ctlClient,ctlClient.Handle),560,mainMenu.Handle,IntPtr.Zero);
          if(menu!=null)
          {
            UnsafeNativeMethods.SendMessage(new HandleRef(ctlClient,ctlClient.Handle),560,menu.Handle,IntPtr.Zero);
          }
        }

        if(menu==null&&mainMenuStrip!=null)
        {
          IntPtr menu2 = UnsafeNativeMethods.GetMenu(new HandleRef(this,base.Handle));
          if(menu2!=IntPtr.Zero)
          {
            UnsafeNativeMethods.SetMenu(new HandleRef(this,base.Handle),NativeMethods.NullHandleRef);
            Form activeMdiChildInternal = ActiveMdiChildInternal;
            if(activeMdiChildInternal!=null&&activeMdiChildInternal.WindowState==FormWindowState.Maximized)
            {
              activeMdiChildInternal.RecreateHandle();
            }

            CommonProperties.xClearPreferredSizeCache(this);
          }
        }
      }

      if(forceRedraw)
      {
        SafeNativeMethods.DrawMenuBar(new HandleRef(this,base.Handle));
      }

      formStateEx[FormStateExUpdateMenuHandlesDeferred]=0;
    }

    internal void UpdateFormStyles()
    {
      Size clientSize = ClientSize;
      UpdateStyles();
      if(!ClientSize.Equals(clientSize))
      {
        ClientSize=clientSize;
      }
    }

    private static Type FindClosestStockType(Type type)
    {
      Type[] array = new Type[1] { typeof(MenuStrip) };
      Type[] array2 = array;
      foreach(Type type2 in array2)
      {
        if(type2.IsAssignableFrom(type))
        {
          return type2;
        }
      }

      return null;
    }

    private void UpdateToolStrip()
    {
      ToolStrip mainMenuStrip = MainMenuStrip;
      ArrayList arrayList = ToolStripManager.FindMergeableToolStrips(ActiveMdiChildInternal);
      if(mainMenuStrip!=null)
      {
        ToolStripManager.RevertMerge(mainMenuStrip);
      }

      UpdateMdiWindowListStrip();
      if(ActiveMdiChildInternal!=null)
      {
        foreach(ToolStrip item in arrayList)
        {
          Type type = FindClosestStockType(item.GetType());
          if(mainMenuStrip!=null)
          {
            Type type2 = FindClosestStockType(mainMenuStrip.GetType());
            if(type2!=null&&type!=null&&type==type2&&mainMenuStrip.GetType().IsAssignableFrom(item.GetType()))
            {
              ToolStripManager.Merge(item,mainMenuStrip);
              break;
            }
          }
        }
      }

      UpdateMdiControlStrip(ActiveMdiChildInternal?.IsMaximized??false);
    }

    private void UpdateMdiControlStrip(bool maximized)
    {
      if(formStateEx[FormStateExInUpdateMdiControlStrip]!=0)
      {
        return;
      }

      formStateEx[FormStateExInUpdateMdiControlStrip]=1;
      try
      {
        MdiControlStrip mdiControlStrip = MdiControlStrip;
        if(MdiControlStrip!=null)
        {
          if(mdiControlStrip.MergedMenu!=null)
          {
            ToolStripManager.RevertMergeInternal(mdiControlStrip.MergedMenu,mdiControlStrip,revertMDIControls: true);
          }

          mdiControlStrip.MergedMenu=null;
          mdiControlStrip.Dispose();
          MdiControlStrip=null;
        }

        if(ActiveMdiChildInternal==null||!maximized||!ActiveMdiChildInternal.ControlBox||Menu!=null)
        {
          return;
        }

        IntPtr menu = UnsafeNativeMethods.GetMenu(new HandleRef(this,base.Handle));
        if(menu==IntPtr.Zero)
        {
          MenuStrip mainMenuStrip = ToolStripManager.GetMainMenuStrip(this);
          if(mainMenuStrip!=null)
          {
            MdiControlStrip=new MdiControlStrip(ActiveMdiChildInternal);
            ToolStripManager.Merge(MdiControlStrip,mainMenuStrip);
            MdiControlStrip.MergedMenu=mainMenuStrip;
          }
        }
      }
      finally
      {
        formStateEx[FormStateExInUpdateMdiControlStrip]=0;
      }
    }

    internal void UpdateMdiWindowListStrip()
    {
      if(!IsMdiContainer)
      {
        return;
      }

      if(MdiWindowListStrip!=null&&MdiWindowListStrip.MergedMenu!=null)
      {
        ToolStripManager.RevertMergeInternal(MdiWindowListStrip.MergedMenu,MdiWindowListStrip,revertMDIControls: true);
      }

      MenuStrip mainMenuStrip = ToolStripManager.GetMainMenuStrip(this);
      if(mainMenuStrip!=null&&mainMenuStrip.MdiWindowListItem!=null)
      {
        if(MdiWindowListStrip==null)
        {
          MdiWindowListStrip=new MdiWindowListStrip();
        }

        int count = mainMenuStrip.MdiWindowListItem.DropDownItems.Count;
        bool includeSeparator = count>0&&!(mainMenuStrip.MdiWindowListItem.DropDownItems[count-1] is ToolStripSeparator);
        MdiWindowListStrip.PopulateItems(this,mainMenuStrip.MdiWindowListItem,includeSeparator);
        ToolStripManager.Merge(MdiWindowListStrip,mainMenuStrip);
        MdiWindowListStrip.MergedMenu=mainMenuStrip;
      }
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Form.ResizeBegin event.
    //
    // Parameters:
    //   e:
    //     A System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void OnResizeBegin(EventArgs e)
    {
      if(CanRaiseEvents)
      {
        ((EventHandler)base.Events[EVENT_RESIZEBEGIN])?.Invoke(this,e);
      }
    }

    //
    // Summary:
    //     Raises the System.Windows.Forms.Form.ResizeEnd event.
    //
    // Parameters:
    //   e:
    //     A System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected virtual void OnResizeEnd(EventArgs e)
    {
      if(CanRaiseEvents)
      {
        ((EventHandler)base.Events[EVENT_RESIZEEND])?.Invoke(this,e);
      }
    }

    //
    // Parameters:
    //   e:
    //     An System.EventArgs that contains the event data.
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    protected override void OnStyleChanged(EventArgs e)
    {
      base.OnStyleChanged(e);
      AdjustSystemMenu();
    }

    private void UpdateWindowIcon(bool redrawFrame)
    {
      if(!base.IsHandleCreated)
      {
        return;
      }

      Icon icon = (((FormBorderStyle!=FormBorderStyle.FixedDialog||formState[FormStateIconSet]!=0||IsRestrictedWindow)&&ShowIcon) ? Icon : null);
      if(icon!=null)
      {
        if(smallIcon==null)
        {
          try
          {
            smallIcon=new Icon(icon,SystemInformation.SmallIconSize);
          }
          catch
          {
          }
        }

        if(smallIcon!=null)
        {
          SendMessage(128,0,smallIcon.Handle);
        }

        SendMessage(128,1,icon.Handle);
      }
      else
      {
        SendMessage(128,0,0);
        SendMessage(128,1,0);
      }

      if(redrawFrame)
      {
        SafeNativeMethods.RedrawWindow(new HandleRef(this,base.Handle),null,NativeMethods.NullHandleRef,1025);
      }
    }

    private void UpdateWindowState()
    {
      if(!base.IsHandleCreated)
      {
        return;
      }

      FormWindowState windowState = WindowState;
      NativeMethods.WINDOWPLACEMENT placement = default(NativeMethods.WINDOWPLACEMENT);
      placement.length=Marshal.SizeOf(typeof(NativeMethods.WINDOWPLACEMENT));
      UnsafeNativeMethods.GetWindowPlacement(new HandleRef(this,base.Handle),ref placement);
      switch(placement.showCmd)
      {
        case 1:
        case 4:
        case 5:
        case 8:
        case 9:
          if(formState[FormStateWindowState]!=0)
          {
            formState[FormStateWindowState]=0;
          }

          break;
        case 3:
          if(formState[FormStateMdiChildMax]==0)
          {
            formState[FormStateWindowState]=2;
          }

          break;
        case 2:
        case 6:
        case 7:
          if(formState[FormStateMdiChildMax]==0)
          {
            formState[FormStateWindowState]=1;
          }

          break;
      }

      if(windowState==FormWindowState.Normal&&WindowState!=0)
      {
        if(WindowState==FormWindowState.Minimized)
        {
          SuspendLayoutForMinimize();
        }

        restoredWindowBounds.Size=ClientSize;
        formStateEx[FormStateExWindowBoundsWidthIsClientSize]=1;
        formStateEx[FormStateExWindowBoundsHeightIsClientSize]=1;
        restoredWindowBoundsSpecified=BoundsSpecified.Size;
        restoredWindowBounds.Location=Location;
        restoredWindowBoundsSpecified|=BoundsSpecified.Location;
        restoreBounds.Size=Size;
        restoreBounds.Location=Location;
      }

      if(windowState==FormWindowState.Minimized&&WindowState!=FormWindowState.Minimized)
      {
        ResumeLayoutFromMinimize();
      }

      switch(WindowState)
      {
        case FormWindowState.Normal:
          SetState(65536,value: false);
          break;
        case FormWindowState.Minimized:
        case FormWindowState.Maximized:
          SetState(65536,value: true);
          break;
      }

      if(windowState!=WindowState)
      {
        AdjustSystemMenu();
      }
    }

    //
    // Returns:
    //     true if all of the children validated successfully; otherwise, false. If called
    //     from the System.Windows.Forms.Control.Validating or System.Windows.Forms.Control.Validated
    //     event handlers, this method will always return false.
    [EditorBrowsable(EditorBrowsableState.Always)]
    [Browsable(true)]
    public override bool ValidateChildren()
    {
      return base.ValidateChildren();
    }

    //
    // Parameters:
    //   validationConstraints:
    //     Places restrictions on which controls have their System.Windows.Forms.Control.Validating
    //     event raised.
    //
    // Returns:
    //     true if all of the children validated successfully; otherwise, false. If called
    //     from the System.Windows.Forms.Control.Validating or System.Windows.Forms.Control.Validated
    //     event handlers, this method will always return false.
    [EditorBrowsable(EditorBrowsableState.Always)]
    [Browsable(true)]
    public override bool ValidateChildren(ValidationConstraints validationConstraints)
    {
      return base.ValidateChildren(validationConstraints);
    }

    private void WmActivate(ref Message m)
    {
      Application.FormActivated(Modal,activated: true);
      Active=NativeMethods.Util.LOWORD(m.WParam)!=0;
      Application.FormActivated(Modal,Active);
    }

    private void WmEnterSizeMove(ref Message m)
    {
      formStateEx[FormStateExInModalSizingLoop]=1;
      OnResizeBegin(EventArgs.Empty);
    }

    private void WmExitSizeMove(ref Message m)
    {
      formStateEx[FormStateExInModalSizingLoop]=0;
      OnResizeEnd(EventArgs.Empty);
    }

    private void WmCreate(ref Message m)
    {
      base.WndProc(ref m);
      NativeMethods.STARTUPINFO_I sTARTUPINFO_I = new NativeMethods.STARTUPINFO_I();
      UnsafeNativeMethods.GetStartupInfo(sTARTUPINFO_I);
      if(TopLevel&&((uint)sTARTUPINFO_I.dwFlags&(true ? 1u : 0u))!=0)
      {
        switch(sTARTUPINFO_I.wShowWindow)
        {
          case 3:
            WindowState=FormWindowState.Maximized;
            break;
          case 6:
            WindowState=FormWindowState.Minimized;
            break;
        }
      }
    }

    private void WmClose(ref Message m)
    {
      FormClosingEventArgs formClosingEventArgs = new FormClosingEventArgs(CloseReason,cancel: false);
      if(m.Msg!=22)
      {
        if(Modal)
        {
          if(dialogResult==DialogResult.None)
          {
            dialogResult=DialogResult.Cancel;
          }

          CalledClosing=false;
          formClosingEventArgs.Cancel=!CheckCloseDialog(closingOnly: true);
        }
        else
        {
          formClosingEventArgs.Cancel=!Validate(checkAutoValidate: true);
          if(IsMdiContainer)
          {
            FormClosingEventArgs formClosingEventArgs2 = new FormClosingEventArgs(CloseReason.MdiFormClosing,formClosingEventArgs.Cancel);
            Form[] mdiChildren = MdiChildren;
            foreach(Form form in mdiChildren)
            {
              if(form.IsHandleCreated)
              {
                form.OnClosing(formClosingEventArgs2);
                form.OnFormClosing(formClosingEventArgs2);
                if(formClosingEventArgs2.Cancel)
                {
                  formClosingEventArgs.Cancel=true;
                  break;
                }
              }
            }
          }

          Form[] ownedForms = OwnedForms;
          int integer = base.Properties.GetInteger(PropOwnedFormsCount);
          for(int num = integer-1;num>=0;num--)
          {
            FormClosingEventArgs formClosingEventArgs3 = new FormClosingEventArgs(CloseReason.FormOwnerClosing,formClosingEventArgs.Cancel);
            if(ownedForms[num]!=null)
            {
              ownedForms[num].OnFormClosing(formClosingEventArgs3);
              if(formClosingEventArgs3.Cancel)
              {
                formClosingEventArgs.Cancel=true;
                break;
              }
            }
          }

          OnClosing(formClosingEventArgs);
          OnFormClosing(formClosingEventArgs);
        }

        if(m.Msg==17)
        {
          m.Result=(IntPtr)((!formClosingEventArgs.Cancel) ? 1 : 0);
        }

        if(Modal)
        {
          return;
        }
      }
      else
      {
        formClosingEventArgs.Cancel=m.WParam==IntPtr.Zero;
      }

      if(m.Msg==17||formClosingEventArgs.Cancel)
      {
        return;
      }

      IsClosing=true;
      FormClosedEventArgs e;
      if(IsMdiContainer)
      {
        e=new FormClosedEventArgs(CloseReason.MdiFormClosing);
        Form[] mdiChildren2 = MdiChildren;
        foreach(Form form2 in mdiChildren2)
        {
          if(form2.IsHandleCreated)
          {
            form2.OnClosed(e);
            form2.OnFormClosed(e);
          }
        }
      }

      Form[] ownedForms2 = OwnedForms;
      int integer2 = base.Properties.GetInteger(PropOwnedFormsCount);
      for(int num2 = integer2-1;num2>=0;num2--)
      {
        e=new FormClosedEventArgs(CloseReason.FormOwnerClosing);
        if(ownedForms2[num2]!=null)
        {
          ownedForms2[num2].OnClosed(e);
          ownedForms2[num2].OnFormClosed(e);
        }
      }

      e=new FormClosedEventArgs(CloseReason);
      OnClosed(e);
      OnFormClosed(e);
      Dispose();
    }

    private void WmEnterMenuLoop(ref Message m)
    {
      OnMenuStart(EventArgs.Empty);
      base.WndProc(ref m);
    }

    private void WmEraseBkgnd(ref Message m)
    {
      UpdateWindowState();
      base.WndProc(ref m);
    }

    private void WmExitMenuLoop(ref Message m)
    {
      OnMenuComplete(EventArgs.Empty);
      base.WndProc(ref m);
    }

    private void WmGetMinMaxInfo(ref Message m)
    {
      Size minTrack = ((AutoSize&&formStateEx[FormStateExInModalSizingLoop]==1) ? LayoutUtils.UnionSizes(minAutoSize,MinimumSize) : MinimumSize);
      Size maximumSize = MaximumSize;
      Rectangle maximizedBounds = MaximizedBounds;
      if(!minTrack.IsEmpty||!maximumSize.IsEmpty||!maximizedBounds.IsEmpty||IsRestrictedWindow)
      {
        WmGetMinMaxInfoHelper(ref m,minTrack,maximumSize,maximizedBounds);
      }

      if(IsMdiChild)
      {
        base.WndProc(ref m);
      }
    }

    private void WmGetMinMaxInfoHelper(ref Message m,Size minTrack,Size maxTrack,Rectangle maximizedBounds)
    {
      NativeMethods.MINMAXINFO mINMAXINFO = (NativeMethods.MINMAXINFO)m.GetLParam(typeof(NativeMethods.MINMAXINFO));
      if(!minTrack.IsEmpty)
      {
        mINMAXINFO.ptMinTrackSize.x=minTrack.Width;
        mINMAXINFO.ptMinTrackSize.y=minTrack.Height;
        if(maxTrack.IsEmpty)
        {
          Size size = SystemInformation.VirtualScreen.Size;
          if(minTrack.Height>size.Height)
          {
            mINMAXINFO.ptMaxTrackSize.y=int.MaxValue;
          }

          if(minTrack.Width>size.Width)
          {
            mINMAXINFO.ptMaxTrackSize.x=int.MaxValue;
          }
        }
      }

      if(!maxTrack.IsEmpty)
      {
        Size minWindowTrackSize = SystemInformation.MinWindowTrackSize;
        mINMAXINFO.ptMaxTrackSize.x=Math.Max(maxTrack.Width,minWindowTrackSize.Width);
        mINMAXINFO.ptMaxTrackSize.y=Math.Max(maxTrack.Height,minWindowTrackSize.Height);
      }

      if(!maximizedBounds.IsEmpty&&!IsRestrictedWindow)
      {
        mINMAXINFO.ptMaxPosition.x=maximizedBounds.X;
        mINMAXINFO.ptMaxPosition.y=maximizedBounds.Y;
        mINMAXINFO.ptMaxSize.x=maximizedBounds.Width;
        mINMAXINFO.ptMaxSize.y=maximizedBounds.Height;
      }

      if(IsRestrictedWindow)
      {
        mINMAXINFO.ptMinTrackSize.x=Math.Max(mINMAXINFO.ptMinTrackSize.x,100);
        mINMAXINFO.ptMinTrackSize.y=Math.Max(mINMAXINFO.ptMinTrackSize.y,SystemInformation.CaptionButtonSize.Height*3);
      }

      Marshal.StructureToPtr(mINMAXINFO,m.LParam,fDeleteOld: false);
      m.Result=IntPtr.Zero;
    }

    private void WmInitMenuPopup(ref Message m)
    {
      MainMenu mainMenu = (MainMenu)base.Properties.GetObject(PropCurMenu);
      if(mainMenu==null||!mainMenu.ProcessInitMenuPopup(m.WParam))
      {
        base.WndProc(ref m);
      }
    }

    private void WmMenuChar(ref Message m)
    {
      MainMenu mainMenu = (MainMenu)base.Properties.GetObject(PropCurMenu);
      if(mainMenu==null)
      {
        Form form = (Form)base.Properties.GetObject(PropFormMdiParent);
        if(form!=null&&form.Menu!=null)
        {
          UnsafeNativeMethods.PostMessage(new HandleRef(form,form.Handle),274,new IntPtr(61696),m.WParam);
          m.Result=(IntPtr)NativeMethods.Util.MAKELONG(0,1);
          return;
        }
      }

      if(mainMenu!=null)
      {
        mainMenu.WmMenuChar(ref m);
        if(m.Result!=IntPtr.Zero)
        {
          return;
        }
      }

      base.WndProc(ref m);
    }

    private void WmMdiActivate(ref Message m)
    {
      base.WndProc(ref m);
      Form form = (Form)base.Properties.GetObject(PropFormMdiParent);
      if(form!=null)
      {
        if(base.Handle==m.WParam)
        {
          form.DeactivateMdiChild();
        }
        else if(base.Handle==m.LParam)
        {
          form.ActivateMdiChildInternal(this);
        }
      }
    }

    private void WmNcButtonDown(ref Message m)
    {
      if(IsMdiChild)
      {
        Form form = (Form)base.Properties.GetObject(PropFormMdiParent);
        if(form.ActiveMdiChildInternal==this&&base.ActiveControl!=null&&!base.ActiveControl.ContainsFocus)
        {
          base.InnerMostActiveContainerControl.FocusActiveControlInternal();
        }
      }

      base.WndProc(ref m);
    }

    private void WmNCDestroy(ref Message m)
    {
      MainMenu menu = Menu;
      MainMenu mainMenu = (MainMenu)base.Properties.GetObject(PropDummyMenu);
      MainMenu mainMenu2 = (MainMenu)base.Properties.GetObject(PropCurMenu);
      MainMenu mainMenu3 = (MainMenu)base.Properties.GetObject(PropMergedMenu);
      menu?.ClearHandles();
      mainMenu2?.ClearHandles();
      mainMenu3?.ClearHandles();
      mainMenu?.ClearHandles();
      base.WndProc(ref m);
      if(ownerWindow!=null)
      {
        ownerWindow.DestroyHandle();
        ownerWindow=null;
      }

      if(Modal&&dialogResult==DialogResult.None)
      {
        DialogResult=DialogResult.Cancel;
      }
    }

    private void WmNCHitTest(ref Message m)
    {
      if(formState[FormStateRenderSizeGrip]!=0)
      {
        int num = NativeMethods.Util.LOWORD(m.LParam);
        int num2 = NativeMethods.Util.HIWORD(m.LParam);
        NativeMethods.POINT pOINT = new NativeMethods.POINT(num,num2);
        UnsafeNativeMethods.ScreenToClient(new HandleRef(this,base.Handle),pOINT);
        Size clientSize = ClientSize;
        if(pOINT.x>=clientSize.Width-16&&pOINT.y>=clientSize.Height-16&&clientSize.Height>=16)
        {
          m.Result=(base.IsMirrored ? ((IntPtr)16) : ((IntPtr)17));
          return;
        }
      }

      base.WndProc(ref m);
      if(AutoSizeMode==AutoSizeMode.GrowAndShrink)
      {
        int num3 = (int)m.Result;
        if(num3>=10&&num3<=17)
        {
          m.Result=(IntPtr)18;
        }
      }
    }

    private void WmShowWindow(ref Message m)
    {
      formState[FormStateSWCalled]=1;
      base.WndProc(ref m);
    }

    private void WmSysCommand(ref Message m)
    {
      bool flag = true;
      switch(NativeMethods.Util.LOWORD(m.WParam)&0xFFF0)
      {
        case 61536:
          CloseReason=CloseReason.UserClosing;
          if(IsMdiChild&&!ControlBox)
          {
            flag=false;
          }

          break;
        case 61696:
          if(IsMdiChild&&!ControlBox)
          {
            flag=false;
          }

          break;
        case 61440:
        case 61456:
          formStateEx[FormStateExInModalSizingLoop]=1;
          break;
        case 61824:
        {
          CancelEventArgs cancelEventArgs = new CancelEventArgs(cancel: false);
          OnHelpButtonClicked(cancelEventArgs);
          if(cancelEventArgs.Cancel)
          {
            flag=false;
          }
          break;
        }
      }

      if(Command.DispatchID(NativeMethods.Util.LOWORD(m.WParam)))
      {
        flag=false;
      }

      if(flag)
      {
        base.WndProc(ref m);
      }
    }

    private void WmSize(ref Message m)
    {
      if(ctlClient==null)
      {
        base.WndProc(ref m);
        if(MdiControlStrip==null&&MdiParentInternal!=null&&MdiParentInternal.ActiveMdiChildInternal==this)
        {
          int num = m.WParam.ToInt32();
          MdiParentInternal.UpdateMdiControlStrip(num==2);
        }
      }
    }

    private void WmUnInitMenuPopup(ref Message m)
    {
      if(Menu!=null)
      {
        Menu.OnCollapse(EventArgs.Empty);
      }
    }

    private void WmWindowPosChanged(ref Message m)
    {
      UpdateWindowState();
      base.WndProc(ref m);
      RestoreWindowBoundsIfNecessary();
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [SecurityPermission(SecurityAction.LinkDemand,Flags = SecurityPermissionFlag.UnmanagedCode)]
    protected override void WndProc(ref Message m)
    {
      switch(m.Msg)
      {
        case 134:
          if(IsRestrictedWindow)
          {
            BeginInvoke(new MethodInvoker(RestrictedProcessNcActivate));
          }

          base.WndProc(ref m);
          break;
        case 161:
        case 164:
        case 167:
        case 171:
          WmNcButtonDown(ref m);
          break;
        case 6:
          WmActivate(ref m);
          break;
        case 546:
          WmMdiActivate(ref m);
          break;
        case 16:
          if(CloseReason==CloseReason.None)
          {
            CloseReason=CloseReason.TaskManagerClosing;
          }

          WmClose(ref m);
          break;
        case 17:
        case 22:
          CloseReason=CloseReason.WindowsShutDown;
          WmClose(ref m);
          break;
        case 561:
          WmEnterSizeMove(ref m);
          DefWndProc(ref m);
          break;
        case 562:
          WmExitSizeMove(ref m);
          DefWndProc(ref m);
          break;
        case 1:
          WmCreate(ref m);
          break;
        case 20:
          WmEraseBkgnd(ref m);
          break;
        case 279:
          WmInitMenuPopup(ref m);
          break;
        case 293:
          WmUnInitMenuPopup(ref m);
          break;
        case 288:
          WmMenuChar(ref m);
          break;
        case 130:
          WmNCDestroy(ref m);
          break;
        case 132:
          WmNCHitTest(ref m);
          break;
        case 24:
          WmShowWindow(ref m);
          break;
        case 5:
          WmSize(ref m);
          break;
        case 274:
          WmSysCommand(ref m);
          break;
        case 36:
          WmGetMinMaxInfo(ref m);
          break;
        case 71:
          WmWindowPosChanged(ref m);
          break;
        case 529:
          WmEnterMenuLoop(ref m);
          break;
        case 530:
          WmExitMenuLoop(ref m);
          break;
        case 533:
          base.WndProc(ref m);
          if(base.CaptureInternal&&Control.MouseButtons==MouseButtons.None)
          {
            base.CaptureInternal=false;
          }

          break;
        default:
          base.WndProc(ref m);
          break;
      }
    }
  }
}
#if false // Decompilation log
'9' items in cache
------------------
Resolve: 'mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Windows\Microsoft.NET\Framework64\v2.0.50727\mscorlib.dll'
------------------
Resolve: 'System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Windows\Microsoft.NET\Framework64\v2.0.50727\System.dll'
------------------
Resolve: 'System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Windows\Microsoft.NET\Framework64\v2.0.50727\System.Drawing.dll'
------------------
Resolve: 'System.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Could not find by name: 'System.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
------------------
Resolve: 'Accessibility, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Could not find by name: 'Accessibility, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
------------------
Resolve: 'System.Configuration, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Could not find by name: 'System.Configuration, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
------------------
Resolve: 'System.Xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Windows\Microsoft.NET\Framework64\v2.0.50727\System.Xml.dll'
------------------
Resolve: 'System.Runtime.Serialization.Formatters.Soap, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Could not find by name: 'System.Runtime.Serialization.Formatters.Soap, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
------------------
Resolve: 'System.Deployment, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Deployment, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Windows\Microsoft.NET\Framework64\v2.0.50727\System.Deployment.dll'
#endif

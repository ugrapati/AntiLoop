#region Assembly System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// C:\Windows\Microsoft.NET\Framework64\v2.0.50727\System.Windows.Forms.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.ComponentModel;
using System.IO;

namespace System.Windows.Forms
{
  //
  // Summary:
  //     Specifies the contextual information about an application thread.
  public class ApplicationContext:IDisposable
  {
    private Form mainForm;

    private object userData;

    //
    // Summary:
    //     Gets or sets the System.Windows.Forms.Form to use as context.
    //
    // Returns:
    //     The System.Windows.Forms.Form to use as context.
    public Form MainForm
    {
      get
      {
        return mainForm;
      }
      set
      {
        EventHandler value2 = OnMainFormDestroy;
        if(mainForm!=null)
        {
          mainForm.HandleDestroyed-=value2;
        }

        mainForm=value;
        if(mainForm!=null)
        {
          mainForm.HandleDestroyed+=value2;
        }
      }
    }

    //
    // Summary:
    //     Gets or sets an object that contains data about the control.
    //
    // Returns:
    //     An System.Object that contains data about the control. The default is null.
    [DefaultValue(null)]
    [TypeConverter(typeof(StringConverter))]
    [Localizable(false)]
    [Bindable(true)]
    [SRDescription("ControlTagDescr")]
    [SRCategory("CatData")]
    public object Tag
    {
      get
      {
        return userData;
      }
      set
      {
        userData=value;
      }
    }

    //
    // Summary:
    //     Occurs when the message loop of the thread should be terminated, by calling System.Windows.Forms.ApplicationContext.ExitThread.
    public event EventHandler ThreadExit;

    //
    // Summary:
    //     Initializes a new instance of the System.Windows.Forms.ApplicationContext class
    //     with no context.
    public ApplicationContext()
      : this(null)
    {
    }

    //
    // Summary:
    //     Initializes a new instance of the System.Windows.Forms.ApplicationContext class
    //     with the specified System.Windows.Forms.Form.
    //
    // Parameters:
    //   mainForm:
    //     The main System.Windows.Forms.Form of the application to use for context.
    public ApplicationContext(Form mainForm)
    {
      MainForm=mainForm;
    }

    //
    // Summary:
    //     Attempts to free resources and perform other cleanup operations before the application
    //     context is reclaimed by garbage collection.
    ~ApplicationContext()
    {
      Dispose(disposing: false);
    }

    //
    // Summary:
    //     Releases all resources used by the System.Windows.Forms.ApplicationContext.
    public void Dispose()
    {
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    //
    // Summary:
    //     Releases the unmanaged resources used by the System.Windows.Forms.ApplicationContext
    //     and optionally releases the managed resources.
    //
    // Parameters:
    //   disposing:
    //     true to release both managed and unmanaged resources; false to release only unmanaged
    //     resources.
    protected virtual void Dispose(bool disposing)
    {
      if(disposing&&mainForm!=null)
      {
        if(!mainForm.IsDisposed)
        {
          mainForm.Dispose();
        }

        mainForm=null;
      }
    }

    //
    // Summary:
    //     Terminates the message loop of the thread.
    public void ExitThread()
    {
      ExitThreadCore();
    }

    //
    // Summary:
    //     Terminates the message loop of the thread.
    protected virtual void ExitThreadCore()
    {
      if(this.ThreadExit!=null)
      {
        this.ThreadExit(this,EventArgs.Empty);
      }
    }

    //
    // Summary:
    //     Calls System.Windows.Forms.ApplicationContext.ExitThreadCore, which raises the
    //     System.Windows.Forms.ApplicationContext.ThreadExit event.
    //
    // Parameters:
    //   sender:
    //     The object that raised the event.
    //
    //   e:
    //     The System.EventArgs that contains the event data.
    protected virtual void OnMainFormClosed(object sender,EventArgs e)
    {
      ExitThreadCore();
    }

    private void OnMainFormDestroy(object sender,EventArgs e)
    {
      Form form = (Form)sender;
      if(!form.RecreatingHandle)
      {
        form.HandleDestroyed-=OnMainFormDestroy;
        OnMainFormClosed(sender,e);
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

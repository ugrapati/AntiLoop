using AntiLoop.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Deployment.Internal.Isolation;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using System.Windows.Forms.VisualStyles;
using Microsoft.Win32;
using static AntiLoop.AntiLoop;

namespace AntiLoop
{ internal static class Program  
  { [STAThread]static void Main()
    { // Configuration conf=ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new AntiLoop());
    }
  }
  public class procList:BindingList<procInfo>{}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Linq;
using AntiLoop.Properties;
using System.ComponentModel.Design;
using System.Windows.Forms.VisualStyles;
using System.Resources;
using System.Threading;
using System.Security.Cryptography;
using System.Runtime.Remoting.Messaging;

namespace AntiLoop
{ public partial class AntiLoop:Form
  { // constants
    [DllImport("USER32",EntryPoint="SendMessage")]public static extern IntPtr sm(IntPtr hWnd,int msg,UIntPtr wParam,IntPtr lParam);
    [DllImport("KERNEL32",EntryPoint="CloseHandle")]private static extern void cl([In]IntPtr h);
    [DllImport("KERNEL32",EntryPoint="CreateFile")]private static extern IntPtr cf([In]string FileName,[In]uint Access,[In]int ShareMode,[In]IntPtr Security_0,[In]int CreationDisposition,[In]int Attributes,[In]IntPtr Template_0);
    [DllImport("KERNEL32",EntryPoint="K32GetModuleFileNameExW")]private static extern int gmf([In]IntPtr ph,[In]IntPtr mh,[In,Out,MarshalAs(UnmanagedType.LPArray,SizeParamIndex=3)]byte[]fn,[In]int nb);
    [DllImport("KERNEL32",EntryPoint="OpenProcess")]private static extern IntPtr op([In]uint acc,[In]int inherit,[In]IntPtr pid);
    [DllImport("KERNEL32",EntryPoint="WaitNamedPipe")]private static extern bool wnp(string pipe,uint timeOut);
    [DllImport("NTDLL",EntryPoint="NtQuerySystemInformation")]private static extern int qsi([In]int cls,[Out,MarshalAs(UnmanagedType.LPArray,SizeParamIndex=2)]byte[]nfo,[In]int len,[In,Out]ref int retLen);
    private static bool wnp(string pipe){IntPtr f=cf("\\\\.\\Pipe\\"+pipe,0xC01F0000,3,(IntPtr)0,3,0,(IntPtr)0);if(((uint)f)==0xFFFFFFFF)return false;cl(f);return true;}
    public static byte[] qsi(int cls,int xt){int n=0;byte[]b;while(qsi(cls,b=new byte[n+xt],b.Length,ref n)==-0x3FFFFFFC&&n>b.Length);Array.Resize(ref b,n);return b;}
    // public static ulong cpuTime{get{byte[]b=qsi(8,0);ulong n=0;for(int i=0;i<b.Length;i+=48)n+=r8u(b,i)+r8u(b,i+8)+r8u(b,i+16);ulong r=n-_lastCpuTime;_lastCpuTime=n;return r;}}
    // private static ulong _lastCpuTime=0; // last total CPU clock cycles for all the CPU cores
    public static int sz=IntPtr.Size,bw=sz*8;
    public static Encoding iso=Encoding.GetEncoding(28591),uni=Encoding.Unicode;
    public const string myPath="C:\\ProgramData\\AntiLoop\\";
    public static DirectoryInfo home=new DirectoryInfo(myPath); // Environment.GetFolderPath((Environment.SpecialFolder)26)+"\\AntiLoop");
    public static byte[]gb(int    o){return BitConverter.GetBytes(o);}
    public static byte[]gb(uint   o){return BitConverter.GetBytes(o);}
    public static byte[]gb(bool   o){return BitConverter.GetBytes(o);}
    public static byte[]gb(char   o){return BitConverter.GetBytes(o);}
    public static byte[]gb(long   o){return BitConverter.GetBytes(o);}
    public static byte[]gb(ulong  o){return BitConverter.GetBytes(o);}
    public static byte[]gb(float  o){return BitConverter.GetBytes(o);}
    public static byte[]gb(short  o){return BitConverter.GetBytes(o);}
    public static byte[]gb(ushort o){return BitConverter.GetBytes(o);}
    public static byte[]gb(double o){return BitConverter.GetBytes(o);}
    public static short   r2i(byte[]b,int i){return  BitConverter.ToInt16(b,i);}
    public static ushort  r2u(byte[]b,int i){return BitConverter.ToUInt16(b,i);}
    public static int     r4i(byte[]b,int i){return  BitConverter.ToInt32(b,i);}
    public static uint    r4u(byte[]b,int i){return BitConverter.ToUInt32(b,i);}
    public static long    r8i(byte[]b,int i){return  BitConverter.ToInt64(b,i);}
    public static ulong   r8u(byte[]b,int i){return BitConverter.ToUInt64(b,i);}
    public static string  rsi(byte[]b,int i){int j=i;for(;j<b.Length&&b[j]!=0;j++);return iso.GetString(b,i,j-i);}
    public static string  rsu(byte[]b,int i){int j=i;for(;j+1<b.Length&&(b[j]!=0||b[j+1]!=0);j+=2);return uni.GetString(b,i,j-i);}
    public static string  rsi(byte[]b,int i,int n){return iso.GetString(b,i,n);}
    public static string  rsu(byte[]b,int i,int n){return uni.GetString(b,i,n);}
    public static IntPtr  rxi(byte[]b,int i){return bw>32?(IntPtr)BitConverter.ToInt64(b,i):(IntPtr)BitConverter.ToInt32(b,i);}
    public static UIntPtr rxu(byte[]b,int i){return bw>32?(UIntPtr)BitConverter.ToUInt64(b,i):(UIntPtr)BitConverter.ToUInt32(b,i);}
    public static void w4(byte[]b,int i,uint o){Array.Copy(BitConverter.GetBytes(o),0,b,i,4);}
    public static void w8(byte[]b,int i,long o){Array.Copy(BitConverter.GetBytes(o),0,b,i,8);}
    public static string[] delayName_text={"Sleep(0) sleep [0.0001..100] (a must; usually 0.1..1)","PeekMessage sleep [1..100] (a must; usually 1..10)","MsgWaitMO(0) sleep [1.100] (avoid or 1)","GetTickCount sleep [0.0001..100] (avoid or 0.0001)","QueryPerfCount sleep [0.0001..100] (avoid or 0.0001)"}; // process filter
    public const string fix_off="   ",fix_on_=" n "; // Fix DISABLED/ENABLED
    public const string x32dll="AntiLoop_32.dll",x64dll="AntiLoop_64.dll";
    public static string X32DLL=x32dll.ToUpper(),X64DLL=x64dll.ToUpper();
    public static string[]fix_text={fix_off,fix_on_};

    public const byte SL0_CNT= 0; // value index; 8 bytes follows
    public const byte PMS_CNT= 1; // value index; 8 bytes follows
    public const byte MWM_CNT= 2; // value index; 8 bytes follows
    public const byte GTC_CNT= 3; // value index; 8 bytes follows
    public const byte QPC_CNT= 4; // value index; 8 bytes follows

    public const byte SL0_REQ= 5; // value index; 8 bytes follows
    public const byte PMS_REQ= 6; // value index; 4 bytes follows
    public const byte MWM_REQ= 7; // value index; 4 bytes follows
    public const byte GTC_REQ= 8; // value index; 8 bytes follows
    public const byte QPC_REQ= 9; // value index; 8 bytes follows

    public const byte SL0_FIX=10; // value index; 1 byte  follows
    public const byte PMS_FIX=11; // value index; 1 byte  follows
    public const byte MWM_FIX=12; // value index; 1 byte  follows
    public const byte GTC_FIX=13; // value index; 8 bytes follows
    public const byte QPC_FIX=14; // value index; 8 bytes follows

    public const byte SEND_DATA=255; // 0..14 = value index; new value follows
    public const int SEND_SIZE=9; // 8 qwords + 2 dwords + 6 bytes
    public const int DATA_SIZE=78; // 8 qwords + 2 dwords + 6 bytes
    // variables
    public static procList pa=new procList(),ps=new procList();
    public static Hashtable ah=new Hashtable(); // ah[pid] -> procInfo; one per process
    public static int x32_active=0; // AntiLoop_32.dll is active; one per registry
    public static int x64_active=0; // AntiLoop_64.dll is active; one per registry
    private static byte[] buf64K=new byte[65536]; // Windows API calls need buffers; max file path is 32K Unicode chars
    // public static string myPath=Path.GetDirectoryName(rsu(buf64K,0,gmf((IntPtr)(-1),IntPtr.Zero,buf64K,buf64K.Length)*2)).TrimEnd('\\')+'\\'; // guaranteed to contain full path of the main module of this process
    public static int cpus=r4i(qsi(0,64),56); // number of CPU cores from SYSTEM_BASIC_INFORMATION.NumberOfProcessors @ SystemBasicInformation
    public bool active=false;
    public IntPtr hwnd=(IntPtr)0;
    public int flip=0; // press space -> OFF; press space -> ON; one per window
    public double idleThreshold=0.00; // 1%
    public string errorLog=""; // one per window

    // classes
    public class procStat
    { public int fix; // fix it
      public ulong cur; // current value
      public ulong prv; // 1 second ago
      public ulong now; // this second
      public ulong max; // peak value
      public double req; // user requested limit in ms
      public procStat() { fix=0; cur=0; prv=0; now=0; max=0; req=0; }
    }

    public class procInfo
    { public long id; // PID (IntPtr actually)
      public bool up; // still running
      public byte bs; // AntiLoop.dll bit size {32|64}
      public IntPtr ph; // process handle
      public int tries;
      public string xpName;
      public byte[]rb=new byte[DATA_SIZE];
      public byte[]wb=new byte[SEND_SIZE];
      public NamedPipeClientStream xp; // named pipe client for data exchange
      public IAsyncResult rres=null,wres=null;
      public int hr; // handle rights: 0 = none, 1 = R, 2 = W, 3 = RW
      public string name; // process name
      public double cpu; // current usage = oldUser/oldTime delta
      public int bad; // < 1% CPU last second => hide it?
      public ulong usd; // usr now - usr a second ago
      public ulong usr; // usr a second ago
      public ulong tot; // krn+usr+idl a second ago
      public procStat sl0; // Sleep(0) stats
      public procStat pms; // PeekMessage() stats
      public procStat mwm; // MsgWaitMultipleObjectsEx(0) stats
      public procStat gtc; // GetTickCounter() stats
      public procStat qpc; // QueryPerformanceCounter() stats
      public long    PID{get{return id;}}
      public string  CPU{get{return cpu!=0?(cpu*100).ToString("N2"):"";}}
      public string NAME{get{return name;}set{name=value;}}
      public string SZ{get{return bs==32?" 32 ":bs==64?" 64 ":"";}}
      public string sl0Fix{get{return AntiLoop.fix_text[sl0.fix];}}
      public string pmsFix{get{return AntiLoop.fix_text[pms.fix];}}
      public string sl0Val{get{return sl0.max==0?"":sl0.now.ToString().PadLeft(8)+" / "+sl0.max.ToString().PadLeft(8);}}
      public string pmsVal{get{return pms.max==0?"":pms.now.ToString().PadLeft(8)+" / "+pms.max.ToString().PadLeft(8);}}
      public string mwmVal{get{return mwm.max==0?"":mwm.now.ToString().PadLeft(8)+" / "+mwm.max.ToString().PadLeft(8);}}
      public string gtcVal{get{return gtc.max==0?"":gtc.now.ToString().PadLeft(8)+" / "+gtc.max.ToString().PadLeft(8);}}
      public string qpcVal{get{return qpc.max==0?"":qpc.now.ToString().PadLeft(8)+" / "+qpc.max.ToString().PadLeft(8);}}

      public procInfo(long _id,string _name)
      { sl0=new procStat();pms=new procStat();mwm=new procStat();gtc=new procStat();qpc=new procStat();
        id=_id;name=_name;up=true;cpu=0;tries=0;xpName="AntiLoop-"+_id.ToString("X8");loadConfig();
        ph=op(0xDF01000,0,(IntPtr)_id); // 0xDF01000 is bare minimum; use 0x80000000 if you want module names (too slow though)
        xp=new NamedPipeClientStream(".",xpName,PipeDirection.InOut,PipeOptions.Asynchronous|PipeOptions.WriteThrough);
      }

      public void loadConfig()
      { if(name==null||name.Length==0)return;byte[]b=new byte[40];string dat=home.FullName+"\\"+name+".dat";
        bool ok=(new FileInfo(dat)).Exists;if(ok)
        { FileStream o=null;try{o=File.Open(dat,FileMode.Open,FileAccess.Read,FileShare.ReadWrite);}catch{}
          if(o!=null){try{ok=o.Read(b,0,40)==40;}catch{ok=false;}o.Close();}
        } if(ok)
        { sl0.req=((double)-r8i(b,0))/10000;
          pms.req=r4u(b,8);
          mwm.req=r4u(b,12);
          gtc.req=((double)-r8i(b,16))/10000;
          qpc.req=((double)-r8i(b,24))/10000;
          sl0.fix=b[32];
          pms.fix=b[33];
          mwm.fix=b[34];
          gtc.fix=b[35];
          qpc.fix=b[36];
        } else // defaults
        { sl0.req=0.3;    // Sleep(0); 0.3 to be safe
          pms.req=1;      // PeekMessage; 1 to be safe
          mwm.req=1;      // MsgWaitMultObj(0); 1
          gtc.req=0.0001; // no fix
          qpc.req=0.0001; // no fix
        }
      }

      public void saveConfig()
      { if(name!=null&&name.Length>0)
        { if(!home.Exists)home.Create();string dat=myPath+name+".dat";
          FileStream o=null;try{o=File.Create(dat);}catch{}if(o!=null)
          { byte[]b=new byte[40];
            w8(b,0,(long)(-sl0.req*10000));
            w4(b,8,(uint)(pms.req));
            w4(b,12,(uint)(mwm.req));
            w8(b,16,(long)(-gtc.req*10000));
            w8(b,24,(long)(-qpc.req*10000));
            b[32]=(byte)sl0.fix;
            b[33]=(byte)pms.fix;
            b[34]=(byte)mwm.fix;
            b[35]=(byte)gtc.fix;
            b[36]=(byte)qpc.fix;
            try{o.Write(b,0,40);}catch{}o.Close();
          }
        }
      }

      // tell that process to update that value per user request
      public void sendValue(byte x,Object v)
      { if(xp.IsConnected)
        { ulong u;long l;byte[]b=new byte[9];b[0]=x;switch(x)
          { case SL0_CNT:case PMS_CNT:case MWM_CNT:case GTC_CNT:case QPC_CNT:u=(ulong)v;for(int i=0;i<8;i++){b[i+1]=(byte)u;u>>=8;}break;
            case SL0_REQ:/*case GTC_REQ:case QPC_REQ:*/l=(long)v;for(int i=0;i<8;i++){b[i+1]=(byte)(l&255);l>>=8;}break;
            case PMS_REQ:/*case MWM_REQ:*/u=(uint)v;b[1]=(byte)u;b[2]=(byte)(u>>8);b[3]=(byte)(u>>16);b[4]=(byte)(u>>24);b[5]=0;b[6]=0;b[7]=0;b[8]=0;break;
            case SL0_FIX:case PMS_FIX:/*case MWM_FIX:case GTC_FIX:case QPC_FIX:*/b[1]=(byte)(int)v;b[2]=0;b[3]=0;b[4]=0;b[5]=0;b[6]=0;b[7]=0;b[8]=0;break;
            default:throw new Exception();
          } try{if(wres!=null&&!wres.IsCompleted)xp.EndWrite(wres);wres=xp.BeginWrite(b,0,9,null,null);}catch{}
        } saveConfig();
      }
    }

    public class procList:BindingList<procInfo>
    { [DllImport("KERNEL32",EntryPoint="K32EnumProcessModulesEx")]private static extern void epm(IntPtr ph,[In,Out,MarshalAs(UnmanagedType.LPArray)]IntPtr[]mh,int bs,ref int need,uint flt); // 0 = auto, 1 = x32, 2 = x64, 3 = all; DUMB USELESS parameter - you can't load a 32-bit module into a 64-bit process and vice-versa
      public static IntPtr[] epm(IntPtr ph){if(ph==null)return new IntPtr[0];int need=IntPtr.Size;IntPtr[]mod=new IntPtr[1];epm(ph,mod,need,ref need,3);if(need==0)return new IntPtr[0];need+=IntPtr.Size;mod=new IntPtr[need/IntPtr.Size];int n=0;epm(ph,mod,need,ref n,3);if(n<need)Array.Resize(ref mod,n/IntPtr.Size);return mod;}
      private static ulong _lastDiff=0,_lastTotal=0; // last total CPU clock cycles for all the CPU cores, per second and total
      private static DateTime _lastTime;
      public static int modNames(IntPtr ph,ref IntPtr[]mod,ref string[]nam){mod=epm(ph);int n=mod.Length;nam=new string[n];for(int i=0;i<n;i++)nam[i]=rsu(buf64K,0,gmf(ph,mod[i],buf64K,buf64K.Length)*2);return n;}
      // updates pa list; all processes, both shown and hidden
      public void updateProcList(AntiLoop form,Hashtable ah,procList ps,double idle,DataGridView grid)
      { DateTime now=DateTime.Now;TimeSpan span=now-_lastTime;_lastTime=now; // ulong total=cpuTime;
        byte[]nfo=qsi(5,262144);int e=nfo.Length,i,j,k,l,n,r,t; // scanning SYSTEM_PROCESS_INFORMATION etc. @ SystemProcessInformation
        procInfo p=null;long par,pid;ulong v,usr,krn,idl,tot=0;double u;
        IntPtr[]mod=new IntPtr[0];string[]nam=new string[0];
        for(i=0;i<Count;i++)this[i].up=false;for(i=0,j=e,n=0;i<e;i+=j)
        { j=r4i(nfo,i);if(j==0)j=e-i; // total blob size for this process; why is the last one 0 ???
          t=r4i(nfo,i+4); // number of threads in this process
          k=i+j-r2i(nfo,i+58); // end of data (process name begins there; its byte length is in i+0x38 ushort)
          pid=(long)rxi(nfo,i+56+3*IntPtr.Size); // unique PID lives there
          par=(long)rxi(nfo,i+56+4*IntPtr.Size); // parent PID
          // struct size before the thread list: 0xB8 bytes on x32, 0x100 bytes on x64
          // thread struct size: 0x40 bytes on x32, 0x50 bytes on x64
          // after the thread list:
          //   KernelTime is at 0x40 on x32, 0x60 on x64
          //     UserTime is at 0x48 on x32, 0x70 on x64
          //     IdleTime is at 0x50 on x32, 0x80 on x64
          l=i+0x90+26*IntPtr.Size+t*(0x30+4*IntPtr.Size); // l now points at kernel time
          krn=r8u(nfo,l              ); //   user time in CPU clock cycles; actually 128-bit on x64, but that's years of up time... on Windows... yeah, right.
          usr=r8u(nfo,l+2*IntPtr.Size); // kernel time in CPU clock cycles; actually 128-bit on x64, but that's years of up time... on Windows... yeah, right.
          idl=r8u(nfo,l+4*IntPtr.Size); //   idle time in CPU clock cycles; actually 128-bit on x64, but that's years of up time... on Windows... yeah, right.
          tot+=usr+krn+idl;             //  total time
          if((p=(procInfo)ah[pid])==null){ah.Add(pid,p=new procInfo(pid,rsu(nfo,k,r2i(nfo,i+0x38))));if(pid>4&&par>4){Add(p);ps.Add(p);}} // ignore system core processes: *, System, Secure System, Registry, Memory Compression and smss.exe
          if(usr!=0) // if the process has been running for more than 30 seconds and it hasn't loaded AntiLoop.dll by now, it won't
          { if(!p.xp.IsConnected&&(usr+krn+idl)<_lastDiff*30&p.tries++<30&&wnp(p.xpName))try{p.xp.Connect(1);}catch{} // try 30 times once per second, then give up
            p.usd=usr-p.usr;p.usr=usr;
          } p.up=true;
        } _lastDiff=tot-_lastTotal;_lastTotal=tot;for(i=0;i<Count;i++)
        { if(!(p=this[i]).up){try{p.xp.Close();}catch{}if(p.ph!=null)cl(p.ph);ah.Remove(p.id);Remove(p);}else
          { u=((double)p.usd)/_lastDiff/cpus;p.bad=(u>=idle)?1:0;
            if((r=ps.IndexOf(p))<0&&p.bad!=0){r=ps.Count;ps.Add(p);}
            if(r>=0&&p.cpu!=u){p.cpu=u;grid.UpdateCellValue(3,r);}
            if(p.xp.IsConnected)try
            { if(p.wres!=null)p.xp.EndWrite(p.wres);
              p.wb[0]=SEND_DATA;Array.Clear(p.wb,1,8);
              p.wres=p.xp.BeginWrite(p.wb,0,9,null,null);
              p.rres=p.xp.BeginRead(p.rb,0,DATA_SIZE,null,null);
            } catch{p.wres=null;p.rres=null;}
          }
        } for(i=0;i<Count;i++) // now that we have the total, we can calculate CPU % usage for each process
        { p=this[i];n=0;if(p.xp.IsConnected&&ps.Contains(p))
          { if(p.wres!=null)try{p.xp.EndWrite(p.wres);p.wres=null;n=-1;}catch{n=0;}
            if(p.rres!=null&&n!=0)try{n=p.xp.EndRead(p.rres);p.rres=null;}catch{n=0;}
            if(n==DATA_SIZE)
            { r=ps.IndexOf(p);
              if(     p.bs!=p.rb[77]){     p.bs=p.rb[77];if(r>=0)grid.UpdateCellValue( 0,r);}
              if(p.sl0.fix!=p.rb[72]){p.sl0.fix=p.rb[72];if(r>=0)grid.UpdateCellValue( 4,r);}
              if(p.pms.fix!=p.rb[73]){p.pms.fix=p.rb[73];if(r>=0)grid.UpdateCellValue( 6,r);}
              p.sl0.cur=r8u(p.rb, 0);p.sl0.req=((double)-r8i(p.rb,40))/10000;
              p.pms.cur=r8u(p.rb, 8);p.pms.req=r4u(p.rb,48);
              p.mwm.cur=r8u(p.rb,16);p.mwm.req=r4u(p.rb,52);
              p.gtc.cur=r8u(p.rb,24);p.gtc.req=((double)-r8i(p.rb,56))/10000;
              p.qpc.cur=r8u(p.rb,32);p.qpc.req=((double)-r8i(p.rb,64))/10000;
              v=(p.sl0.prv!=0)?(ulong)((p.sl0.cur-p.sl0.prv)*(ulong)TimeSpan.TicksPerSecond/(ulong)span.Ticks):0;if(r>=0&&p.sl0.now!=v)grid.UpdateCellValue( 5,r);p.sl0.now=v;p.sl0.prv=p.sl0.cur;if(p.sl0.now>p.sl0.max)p.sl0.max=p.sl0.now;
              v=(p.pms.prv!=0)?(ulong)((p.pms.cur-p.pms.prv)*(ulong)TimeSpan.TicksPerSecond/(ulong)span.Ticks):0;if(r>=0&&p.pms.now!=v)grid.UpdateCellValue( 7,r);p.pms.now=v;p.pms.prv=p.pms.cur;if(p.pms.now>p.pms.max)p.pms.max=p.pms.now;
              v=(p.mwm.prv!=0)?(ulong)((p.mwm.cur-p.mwm.prv)*(ulong)TimeSpan.TicksPerSecond/(ulong)span.Ticks):0;if(r>=0&&p.mwm.now!=v)grid.UpdateCellValue( 8,r);p.mwm.now=v;p.mwm.prv=p.mwm.cur;if(p.mwm.now>p.mwm.max)p.mwm.max=p.mwm.now;
              v=(p.gtc.prv!=0)?(ulong)((p.gtc.cur-p.gtc.prv)*(ulong)TimeSpan.TicksPerSecond/(ulong)span.Ticks):0;if(r>=0&&p.gtc.now!=v)grid.UpdateCellValue( 9,r);p.gtc.now=v;p.gtc.prv=p.gtc.cur;if(p.gtc.now>p.gtc.max)p.gtc.max=p.gtc.now;
              v=(p.qpc.prv!=0)?(ulong)((p.qpc.cur-p.qpc.prv)*(ulong)TimeSpan.TicksPerSecond/(ulong)span.Ticks):0;if(r>=0&&p.qpc.now!=v)grid.UpdateCellValue(10,r);p.qpc.now=v;p.qpc.prv=p.qpc.cur;if(p.qpc.now>p.qpc.max)p.qpc.max=p.qpc.now;
            }
          }
        } // if(grid.CurrentCell.ColumnIndex>3)form.activeCell(grid.CurrentCell.ColumnIndex,grid.CurrentCell.RowIndex);
      }
    }

    // methods
    private int isAntiLoopEnabled(int bits)
    { RegistryKey rk=null;
      rk=Registry.LocalMachine.OpenSubKey("SOFTWARE\\"+(bits<bw?"Wow6432Node\\":"")+"Microsoft\\Windows NT\\CurrentVersion\\Windows",false);
      if(rk.GetValueKind("LoadAppInit_DLLs")==RegistryValueKind.DWord
        &&(int)rk.GetValue("LoadAppInit_DLLs")!=0
        &&rk.GetValueKind("AppInit_DLLs")==RegistryValueKind.String)
        { string[]r=splitPaths((string)rk.GetValue("AppInit_DLLs","")); // split the path list
          string MYDLL=(bits==32)?X32DLL:X64DLL;for(int i=0;i<r.Length;i++)if(Path.GetFileName(r[i].Trim(' ')).ToUpper()==MYDLL&&(new FileInfo(r[i])).Exists){rk.Close();return 1;} // found self
        } if(rk!=null)rk.Close();return 0;
    }

    // enable/disable AntiLoop_32|64.dll
    private int switchAntiLoop(int bits,int active) // bits = 32|64; active = 0|1
    { RegistryKey rk=null;string key="SOFTWARE\\"+(bits<bw?"Wow6432Node\\":"")+"Microsoft\\Windows NT\\CurrentVersion\\Windows";try
      { rk=Registry.LocalMachine.OpenSubKey(key,true);string[]r=new string[0];string MYDLL=(bits==32)?X32DLL:X64DLL,mydll=(bits==32)?x32dll:x64dll;
        if(rk.GetValueKind("AppInit_DLLs")==RegistryValueKind.String)r=splitPaths((string)rk.GetValue("AppInit_DLLs","")); // split the path list
        int i=0;for(;i<r.Length;i++)if(Path.GetFileName(r[i].Trim(' ')).ToUpper()==MYDLL)r[i]=""; // remove self
        string s=((active!=0)?(myPath+mydll):"");for(i=0;i<r.Length;i++)if(r[i].Trim('"').Length!=0){if(s.Length!=0)s+=' ';s+=r[i];}
        rk.SetValue("AppInit_DLLs",s,RegistryValueKind.String);
        rk.SetValue("LoadAppInit_DLLs",active|(s.Length>0?1:0),RegistryValueKind.DWord);
        rk.SetValue("RequireSignedAppInit_DLLs",0,RegistryValueKind.DWord); // now this one is tricky - gotta make it an option
      } catch
      { active=0;errorLog+="No access to "+key+"\r\n - could not turn on "+bits.ToString()+"-bit monitoring.";
      } if(rk!=null)rk.Close();return active;
    }

    // updates proc list in the grid and removes dead processes
    public void updateGrid()
    { pa.updateProcList(this,ah,ps,idleThreshold/100,grid);
      for(int i=0;i<ps.Count;)if(ah[ps[i].id]==null)ps.RemoveAt(i);else i++;
    }

    // order the list by PID/name/CPU %
    private void sortShownList(int col,int rev) // sort by PID in ascending order
    { int i,n=ps.Count;procInfo[]vs=new procInfo[n];switch(col)
      { case  0:{  byte[]ks=new   byte[n];for(i=0;i<n;i++)ks[i]=(vs[i]=ps[i]).bs;     Array.Sort(ks,vs);for(i=0;i<n;i++)ps[i]=vs[(rev==0)?i:n-1-i];break;}
        case  1:{  long[]ks=new   long[n];for(i=0;i<n;i++)ks[i]=(vs[i]=ps[i]).id;     Array.Sort(ks,vs);for(i=0;i<n;i++)ps[i]=vs[(rev==0)?i:n-1-i];break;}
        case  2:{string[]ks=new string[n];for(i=0;i<n;i++)ks[i]=(vs[i]=ps[i]).name;   Array.Sort(ks,vs);for(i=0;i<n;i++)ps[i]=vs[(rev==0)?i:n-1-i];break;}
        case  3:{double[]ks=new double[n];for(i=0;i<n;i++)ks[i]=(vs[i]=ps[i]).cpu;    Array.Sort(ks,vs);for(i=0;i<n;i++)ps[i]=vs[(rev==0)?i:n-1-i];break;}
        case  5:{double[]ks=new double[n];for(i=0;i<n;i++)ks[i]=(vs[i]=ps[i]).sl0.max;Array.Sort(ks,vs);for(i=0;i<n;i++)ps[i]=vs[(rev==0)?i:n-1-i];break;}
        case  7:{double[]ks=new double[n];for(i=0;i<n;i++)ks[i]=(vs[i]=ps[i]).pms.max;Array.Sort(ks,vs);for(i=0;i<n;i++)ps[i]=vs[(rev==0)?i:n-1-i];break;}
        case  8:{double[]ks=new double[n];for(i=0;i<n;i++)ks[i]=(vs[i]=ps[i]).mwm.max;Array.Sort(ks,vs);for(i=0;i<n;i++)ps[i]=vs[(rev==0)?i:n-1-i];break;}
        case  9:{double[]ks=new double[n];for(i=0;i<n;i++)ks[i]=(vs[i]=ps[i]).gtc.max;Array.Sort(ks,vs);for(i=0;i<n;i++)ps[i]=vs[(rev==0)?i:n-1-i];break;}
        case 10:{double[]ks=new double[n];for(i=0;i<n;i++)ks[i]=(vs[i]=ps[i]).qpc.max;Array.Sort(ks,vs);for(i=0;i<n;i++)ps[i]=vs[(rev==0)?i:n-1-i];break;}
      }
    }

    // form width is not resizeable
    private void matchGridWidth()
    { int w=grid.Columns.GetColumnsWidth(DataGridViewElementStates.None)+grid.RowHeadersWidth+56;
      if(Width<w)Width=w;MinimumSize=new Size(w,MinimumSize.Height);
    }

    // Form initialization
    public AntiLoop()
    { if(cpus<1)cpus=1;else if(cpus>64)cpus=64;InitializeComponent();matchGridWidth();
      FileInfo al32=new FileInfo(myPath+x32dll),al64=new FileInfo(myPath+x64dll);if(!home.Exists)home.Create();
      if(!al32.Exists)File.WriteAllBytes(al32.FullName,Resources.AntiLoop_32);x32_active=isAntiLoopEnabled(32);
      on32.BackgroundImage=x32_active!=0?Resources.small5bars:Resources.small0bars;if(bw<64){on64.Hide();x64.Hide();}else
      { if(!al64.Exists)File.WriteAllBytes(al64.FullName,Resources.AntiLoop_64);x64_active=isAntiLoopEnabled(64);
        on64.BackgroundImage=x64_active!=0?Resources.small5bars:Resources.small0bars;
      } hwnd=Handle;grid.DataSource=null;grid.DataSource=ps;updateGrid();hideIdlers();sortShownList(3,1);
      (new Thread(updateLoop)).Start();grid.Select();
    }

    // show more info
    private void grid_CellMouseEnter(object sender,DataGridViewCellEventArgs e)
    { // TODO: show more info
    }

    // validate idleness threshold and hide idlers
    private void hideIdlers()
    { double d;double.TryParse(idlePc.Text,out d);if(d<0)d=0;else if(d>100)d=100;idleThreshold=d;idlePc.Text=d.ToString("N2");
      for(int i=0,r;i<pa.Count;i++)if(pa[i].bad==0&&(r=ps.IndexOf(pa[i]))>=0)ps.RemoveAt(r); // hide pa[i] if it's shown
    }

    // hide idlers clicked
    private void hideIdle_Click(object sender,EventArgs e)
    { hideIdlers();grid.Select();
    }

    // show AntiLoop_32.dll processes
    private void x32_Click(object sender,EventArgs e)
    { procInfo p;for(int i=0,r;i<pa.Count;i++)if((p=pa[i]).bs==32){if(ps.IndexOf(p)<0)ps.Add(p);}else if((r=ps.IndexOf(p))>=0)ps.RemoveAt(r);grid.Select();
    }

    // show AntiLoop_64.dll processes
    private void x64_Click(object sender,EventArgs e)
    { procInfo p;for(int i=0,r;i<pa.Count;i++)if((p=pa[i]).bs==64){if(ps.IndexOf(p)<0)ps.Add(p);}else if((r=ps.IndexOf(p))>=0)ps.RemoveAt(r);grid.Select();
    }

    // show all processes
    private void showAll_Click(object sender,EventArgs e)
    { procInfo p;for(int i=0;i<pa.Count;i++)if(ps.IndexOf(p=pa[i])<0)ps.Add(p);grid.Select();
    }

    // show the limit box
    private void activeCell(int c,int r)
    { if(r>=0)switch(c)
      { case  4:case  5:delayName.Text=delayName_text[(c-4)>>1];delay.Text=ps[r].sl0.req.ToString("0.####");delayName.Visible=delay.Visible=msLabel.Visible=true;break;
        case  6:case  7:delayName.Text=delayName_text[(c-4)>>1];delay.Text=ps[r].pms.req.ToString("0"     );delayName.Visible=delay.Visible=msLabel.Visible=true;break;
        default:delayName.Visible=delay.Visible=msLabel.Visible=false;break;
      } else delayName.Visible=delay.Visible=msLabel.Visible=false;
    }

    // update the top-right text and edit box
    private void grid_CellEnter(object sender,DataGridViewCellEventArgs e)
    { activeCell(e.ColumnIndex,e.RowIndex);
    }

    // on/off selection
    private void grid_KeyDown(object sender,KeyEventArgs e)
    { int flipped=0;if(e.KeyCode==Keys.Space)
      { if(grid.SelectedCells.Count==1) // only one cell selected - flip it as is
        { DataGridViewCell v=grid.SelectedCells[0];int c=v.ColumnIndex,r=v.RowIndex;procInfo p=ps[r];switch(c)
          { case 0:case 1:case 2:case 3:
            p.sendValue(SL0_FIX,p.sl0.fix=flip);grid.UpdateCellValue(4,r);
            p.sendValue(PMS_FIX,p.pms.fix=flip);grid.UpdateCellValue(6,r);flip^=1;break;
            case  4:case  5:flip=p.sl0.fix;p.sendValue(SL0_FIX,p.sl0.fix^=1);grid.UpdateCellValue(4,r);break;
            case  6:case  7:flip=p.pms.fix;p.sendValue(PMS_FIX,p.pms.fix^=1);grid.UpdateCellValue(6,r);break;
          }
        } else
        { foreach (DataGridViewCell v in grid.SelectedCells)
          { int c=v.ColumnIndex,r=v.RowIndex;procInfo p=ps[r];
            if(c<4)
            { p.sendValue(SL0_FIX,p.sl0.fix=flip);grid.UpdateCellValue(4,r);
              p.sendValue(PMS_FIX,p.pms.fix=flip);grid.UpdateCellValue(6,r);
              flipped=1;
            } else switch(c)
            { case  4:case  5:p.sendValue(SL0_FIX,p.sl0.fix=flip);grid.UpdateCellValue(4,r);flipped=1;break;
              case  6:case  7:p.sendValue(PMS_FIX,p.pms.fix=flip);grid.UpdateCellValue(6,r);flipped=1;break;
            }
          } flip^=flipped; // if at least one valid cell got hit, it's ON/OFF flip next time
        }
      } else
      { flip=0; // it's OFF if anything other than space was precced
        if(e.KeyCode>=Keys.D0&&e.KeyCode<=Keys.D9||
           e.KeyCode>=Keys.NumPad0&&e.KeyCode<=Keys.NumPad9)
        { delay.Focus();delay.Text=""+(char)(0x30+(((int)e.KeyCode)&15));delay.SelectionStart=1;delay.SelectionLength=0;
        } if(e.KeyCode==Keys.Escape)Close();
      }
    }

    // new selection => OFF first
    private void grid_SelectionChanged(object sender,EventArgs e)
    { flip=0;if(grid.SelectedCells.Count!=1)delayName.Visible=delay.Visible=msLabel.Visible=false;else
        activeCell(grid.SelectedCells[0].ColumnIndex,grid.SelectedCells[0].RowIndex);
    }

    // can resize only the height of the window; width must match the grid
    private void grid_ColumnWidthChanged(object sender,DataGridViewColumnEventArgs e)
    { matchGridWidth();
    }

    // on/off one cell
    private void grid_CellMouseClick(object sender,DataGridViewCellMouseEventArgs e)
    { int c=e.ColumnIndex,r=e.RowIndex;switch(c)
      { case 4:case 6:if(r<0)
        { for(int i=0;i<grid.Rows.Count;i++)
          { procInfo p=ps[i];switch(c)
            { case  4:p.sendValue(SL0_FIX,p.sl0.fix=flip);break;
              case  6:p.sendValue(PMS_FIX,p.pms.fix=flip);break;
            }
          } flip^=1;
        } else
        { procInfo p=ps[r];switch(c)
          { case  4:p.sendValue(SL0_FIX,p.sl0.fix^=1);break;
            case  6:p.sendValue(PMS_FIX,p.pms.fix^=1);break;
          }
        } break;
        default:if(r<0)sortShownList(c,(c!=1&&c!=2)?1:0);else activeCell(c,r);break;
      }
    }

    // on/off one cell
    private void grid_CellMouseDoubleClick(object sender,DataGridViewCellMouseEventArgs e)
    { grid_CellMouseClick(sender,e);
    }

    // got new idle % threshold
    private void idlePc_Leave(object sender,EventArgs e)
    { idlePc.Text=idleThreshold.ToString("N2");
    }

    // pressed enter; reacting to every edit is very annoying
    private void idlePc_KeyDown(object sender,KeyEventArgs e)
    { switch(e.KeyCode)
      { case Keys.Enter:hideIdlers();grid.Select();break;
        case Keys.Escape:idlePc.Text=idleThreshold.ToString("N2");grid.Select();break;
      }
    }

    private void idlePc_Validating(object sender,CancelEventArgs e)
    {
    }

    private void setNewDelay()
    { double d=0;double.TryParse(delay.Text,out d);if(d>100)d=100;if(grid.SelectedCells.Count==1) // the delay value is different for each row
      { int r=grid.SelectedCells[0].RowIndex,c=grid.SelectedCells[0].ColumnIndex;procInfo p=ps[r];switch(c)
        { case  4:case  5:if(d<.0001)d=.0001;p.sendValue(SL0_REQ,(long)(-d*10000));grid.ClearSelection();break; // delay.Text=d.ToString("0.####");
          case  6:case  7:if(d<    1)d=    1;p.sendValue(PMS_REQ,(uint)d         );grid.ClearSelection();break; // delay.Text=d.ToString("0"     );
         }
      }
    }

    // re-scan processes every second
    public void updateLoop()
    { while(!IsDisposed)
      { if(active){sm(hwnd,0x0100,(UIntPtr)0xFFFF,(IntPtr)0);}
        Thread.Sleep(1000);
      }
    }

    private void AntiLoop_Activated(object sender,EventArgs e){active=true;}
    private void AntiLoop_Deactivate(object sender,EventArgs e){active=false;}

    // main form never receives KeyDown, KeyUp or KeyPress events unless sent explicitly
    private void AntiLoop_KeyDown(object sender,KeyEventArgs e)
    { if(e.KeyValue==0xFFFF){active=false;updateGrid();active=true;}
      else if(e.KeyCode==Keys.Escape)Close();
    }

    // validate the user-edited max calls/sec limit
    private void delay_Leave(object sender,EventArgs e)
    { setNewDelay();
    }

    private void delay_KeyDown(object sender,KeyEventArgs e)
    { if(e.KeyCode==Keys.Enter){setNewDelay();grid.Select();}
    }

    // the AppInit_DLLs string is a list of space-separated paths with or without quotes - the programmer must have been drunk, forgot about MultiString
    private static string[] splitPaths(string s)
    { int i,j=0,n=0;for(i=0;i<s.Length;i++)if(s[i]=='"'){while(++i<s.Length&&s[i]!='"');i++;n++;}else if(s[i]==' '&&i>0&&s[i-1]!=' ')n++;if(i>0&&s[i-1]!=' ')n++;
      string[]r=new string[n];if(n!=0)for(i=0,n=0;i<s.Length;i++)
      { if(s[i]=='"'){while(++i<s.Length&&s[i]!='"');if(i>=s.Length)s+='"';i++;}
        else if(s[i]!=' '||i==0||s[i-1]==' ')continue;r[n]=s.Substring(j,i-j).Trim(' ');j=i+1;n++;
      } if(n<r.Length)r[n]=s.Substring(j,i-j).Trim(' ');return r;
    }

    // install in registry
    private void on32_Click(object sender,EventArgs e)
    { x32_active=switchAntiLoop(32,x32_active^1);
      on32.BackgroundImage=x32_active!=0?Resources.small5bars:Resources.small0bars;grid.Select();
    }

    // install in registry
    private void on64_Click(object sender,EventArgs e)
    { x64_active=switchAntiLoop(64,x64_active^1);
      on64.BackgroundImage=x64_active!=0?Resources.small5bars:Resources.small0bars;grid.Select();
    }
  }
  // public static string rsu (byte[]b,int i){int j=i;for(;j+1<b.Length&&(b[j]!=0||b[j+1]!=0);j+=2);return rsu(b,i,j-i);}
}

// TODO: ESC closes window - done
// TODO: a digit edits delay - done
// TODO: remove x64 from Windows-32 - done
// TODO: find out why some pipes are not responding ...

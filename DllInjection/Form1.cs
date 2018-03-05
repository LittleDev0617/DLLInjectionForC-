using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Text;

namespace DllInjection
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32")]
        public static extern IntPtr LoadLibrary(String fileName);
        [DllImport("kernel32.dll")]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint Alloc_T, uint old_P);
        [DllImport("kernel32.dll")]
        static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, out int w_b);
        [DllImport("kernel32.dll")]
        static extern int CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll")]
        static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        [DllImport("kernel32.dll")]
        static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);
        [DllImport("kernel32")]
        public static extern uint GetLastError();

        static readonly IntPtr INTPTR_ZERO = (IntPtr)0;
        string dll_Path;
        Process notepad;

        public Form1()
        {
            InitializeComponent();
        }

        private void getNotePad()
        {
            foreach (Process pro in Process.GetProcesses())
            {
                if(pro.ProcessName == "notepad")
                {
                    notepad = pro;
                    break;
                }
            }
        }

        private void dllInjection(object o,EventArgs e)
        {
            dll_Path = Application.StartupPath + "\\myHackDLL.dll";
            getNotePad();
            //kernel32.dll 에 있는 LoadLibraryA 함수의 주소를 얻는다.
            IntPtr LoadLibraryAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            if (LoadLibraryAddress == INTPTR_ZERO)
            {
                MsgBox("GetProcAddress에서 오류가 났습니다!");
                return;
            }
            MessageBox.Show(notepad.Id.ToString());
            //메모장 프로세스의 빈 메모리 주소를 0을 할당.                                                 커밋   예약     보호 상수          
            //메모장 프로세스라는 도시.
            //그 도시의 안쓰는 집을 찾아서.
            //집을 비워달라고 요청.
            IntPtr DLLPath_Address = VirtualAllocEx(notepad.Handle, INTPTR_ZERO, (IntPtr)dll_Path.Length, (0x1000 | 0x2000), 0x40);
            if(DLLPath_Address == INTPTR_ZERO)
            {
                MsgBox("VirtualAllocEx에서 오류가 났습니다!");
                return;
            }

            byte[] bytes = Encoding.ASCII.GetBytes(dll_Path);
            int a;

            //빈 집에다가 이삿짐을 옮김.
            //1.도시,2.집주소,3.이삿짐,4.이삿짐의 크기
            if(WriteProcessMemory(notepad.Handle,DLLPath_Address,bytes,(uint)bytes.Length,out a) == 0)
            {
                MsgBox("WriteProcessMemory에서 오류가 났습니다!");
                return;
            }

            if(CreateRemoteThread(notepad.Handle,INTPTR_ZERO, INTPTR_ZERO, LoadLibraryAddress,DLLPath_Address,0,INTPTR_ZERO) == INTPTR_ZERO)
            {
                MsgBox("CreateRemoteThread에서 오류가 났습니다!" + GetLastError().ToString());
                return;
            }
            
            //CloseHandle(notepad.Handle);
            return;
        }
        
        void MsgBox(string msg)
        {
            MessageBox.Show(msg);
        }
    }
}

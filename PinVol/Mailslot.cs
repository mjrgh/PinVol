// C# Mailslot wrapper
//
// By m_karamar
// From the Code Project article:
// https://www.codeproject.com/Articles/623268/Csharp-wrapper-for-Mailslots
//
// Code Project Open License
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace PinVol
{
    [SuppressUnmanagedCodeSecurity]
    public static class Mailslot
    {
        public const int MailslotNoMessage = -1;

        [Flags]
        public enum FileDesiredAccess : uint
        {
            GenericRead = 0x80000000,
            GenericWrite = 0x40000000,
            GenericExecute = 0x20000000,
            GenericAll = 0x10000000
        }

        [Flags]
        public enum FileShareMode : uint
        {
            Zero = 0x00000000,
            FileShareDelete = 0x00000004,
            FileShareRead = 0x00000001,
            FileShareWrite = 0x00000002
        }

        public enum FileCreationDisposition : uint
        {
            CreateNew = 1,
            CreateAlways = 2,
            OpenExisting = 3,
            OpenAlways = 4,
            TruncateExisting = 5
        }

        // .Net 2.0 only - [SecurityCritical(SecurityCriticalScope.Everything)]
        [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public sealed class SafeMailslotHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeMailslotHandle() : base(true) { }
            public SafeMailslotHandle(IntPtr preexistingHandle, bool ownsHandle)
                : base(ownsHandle)
            {
                base.SetHandle(preexistingHandle);
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr handle);

            protected override bool ReleaseHandle()
            {
                return CloseHandle(base.handle);
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeMailslotHandle CreateMailslot(string mailslotName,
                                                    uint nMaxMessageSize, int lReadTimeout,
                                                    IntPtr securityAttributes);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMailslotInfo(SafeMailslotHandle hMailslot,
                                                    IntPtr lpMaxMessageSize,
                                                    out int lpNextSize, out int lpMessageCount,
                                                    IntPtr lpReadTimeout);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReadFile(SafeMailslotHandle handle,
                                            byte[] bytes, int numBytesToRead, out int numBytesRead,
                                            IntPtr overlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteFile(SafeMailslotHandle handle,
                                            byte[] bytes, int numBytesToWrite, out int numBytesWritten,
                                            IntPtr overlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeMailslotHandle CreateFile(string fileName,
                                            FileDesiredAccess desiredAccess, FileShareMode shareMode,
                                            IntPtr securityAttributes,
                                            FileCreationDisposition creationDisposition,
                                            int flagsAndAttributes, IntPtr hTemplateFile);
    }

    public class MailslotServer : IDisposable
    {
        private Mailslot.SafeMailslotHandle _handle;

        public MailslotServer(string name)
        {
            _handle = Mailslot.CreateMailslot(@"\\.\mailslot\" + name, 0, unchecked((int)0xFFFFFFFF), IntPtr.Zero);
            if (_handle.IsInvalid) throw new Win32Exception();
        }

        public string GetNextMessage()
        {
            int messageBytes;
            int bytesRead;
            int messages;

            if (!Mailslot.GetMailslotInfo(_handle, IntPtr.Zero, out messageBytes,
                         out messages, IntPtr.Zero)) throw new Win32Exception();

            if (messageBytes == Mailslot.MailslotNoMessage) return null;

            var bBuffer = new byte[messageBytes];

            if (!Mailslot.ReadFile(_handle, bBuffer, messageBytes, out bytesRead,
                 IntPtr.Zero) || bytesRead == 0) throw new Win32Exception();

            return Encoding.Unicode.GetString(bBuffer);
        }

        public void Dispose()
        {
            if (_handle != null)
            {
                _handle.Close();
                _handle = null;
            }
        }
    }
    public class MailslotClient : IDisposable
    {
        private Mailslot.SafeMailslotHandle _handle;
        private readonly string _name;
        private readonly string _machine;

        public MailslotClient(string name) : this(name, ".") { }
        public MailslotClient(string name, string machine)
        {
            _name = name;
            _machine = machine;
        }

        public void SendMessage(string msg)
        {
            if (_handle == null) CreateHandle();

            int bytesWritten;

            byte[] bMessage = Encoding.Unicode.GetBytes(msg);

            bool succeeded = Mailslot.WriteFile(_handle, bMessage,
                 bMessage.Length, out bytesWritten, IntPtr.Zero);

            if (!succeeded || bMessage.Length != bytesWritten)
            {
                if (_handle != null) _handle.Close();
                _handle = null;

                throw new Win32Exception();
            }
        }
        public void Dispose()
        {
            if (_handle != null)
            {
                _handle.Close();
                _handle = null;
            }
        }

        private void CreateHandle()
        {
            _handle = Mailslot.CreateFile(
                @"\\" + _machine + @"\mailslot\" + _name,
                Mailslot.FileDesiredAccess.GenericWrite,
                Mailslot.FileShareMode.FileShareRead,
                IntPtr.Zero,
                Mailslot.FileCreationDisposition.OpenExisting,
                0,
                IntPtr.Zero);

            if (_handle.IsInvalid) throw new Win32Exception();

        }
    }
}

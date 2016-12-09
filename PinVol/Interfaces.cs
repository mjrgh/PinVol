using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace PinVol
{
    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    class MMDeviceEnumerator
    {
    }

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDeviceEnumerator
    {
        int EnumAudioEndpoints(EDataFlow dataFlow, DeviceState stateMask,
            out IMMDeviceCollection devices);
        
        int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice endpoint);
        
        int GetDevice(string id, out IMMDevice deviceName);
        
        int RegisterEndpointNotificationCallback(IMMNotificationClient client);
        
        int UnregisterEndpointNotificationCallback(IMMNotificationClient client);
    }

    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioEndpointVolume
    {
        int RegisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);
        int UnregisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);
        int GetChannelCount(out int pnChannelCount);
        int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);
        int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);
        int GetMasterVolumeLevel(out float pfLevelDB);
        int GetMasterVolumeLevelScalar(out float pfLevel);
        int SetChannelVolumeLevel(uint nChannel, float fLevelDB, ref Guid pguidEventContext);
        int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, ref Guid pguidEventContext);
        int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);
        int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);
        int SetMute([MarshalAs(UnmanagedType.Bool)] Boolean bMute, ref Guid pguidEventContext);
        int GetMute(out bool pbMute);
        int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
        int VolumeStepUp(ref Guid pguidEventContext);
        int VolumeStepDown(ref Guid pguidEventContext);
        int QueryHardwareSupport(out uint pdwHardwareSupportMask);
        int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
    }

    [Guid("657804FA-D6AD-4496-8A60-352752AF4F89"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioEndpointVolumeCallback
    {
        void OnNotify([MarshalAs(UnmanagedType.Struct)] ref AUDIO_VOLUME_NOTIFICATION_DATA notifyData);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AUDIO_VOLUME_NOTIFICATION_DATA
    {
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType=UnmanagedType.U1, SizeConst=16)]
        public byte[] guidEventContext;
        
        public bool bMuted;
        
        public float fMasterVolume;
        
        public UInt32 nChannels;

        //[MarshalAs(UnmanagedType.ByValArray, SizeConst=nChannels - can't do this!!!)]
        //public float[] afChannelVolumes;
    };

    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDeviceCollection
    {
        int GetCount(out int numDevices);
        int Item(int deviceNumber, out IMMDevice device);
    }

    
    [Guid("D666063F-1587-4E43-81F1-B948E807363F"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDevice
    {
        // activationParams is a propvariant
        int Activate(ref Guid id, ClsCtx clsCtx, IntPtr activationParams,
            [MarshalAs(UnmanagedType.IUnknown)] out object interfacePointer);
        
        int OpenPropertyStore(StorageAccessMode stgmAccess, out IPropertyStore properties);
        
        int GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);
        
        int GetState(out DeviceState state);
    }

    /// <summary>
    /// IMMNotificationClient
    /// </summary>
    [Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0"), 
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMNotificationClient
    {
        /// <summary>
        /// Device State Changed
        /// </summary>
        void OnDeviceStateChanged([MarshalAs(UnmanagedType.LPWStr)] string deviceId, [MarshalAs(UnmanagedType.I4)] DeviceState newState);

        /// <summary>
        /// Device Added
        /// </summary>
        void OnDeviceAdded([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId);

        /// <summary>
        /// Device Removed
        /// </summary>
        void OnDeviceRemoved([MarshalAs(UnmanagedType.LPWStr)] string deviceId);

        /// <summary>
        /// Default Device Changed
        /// </summary>
        void OnDefaultDeviceChanged(EDataFlow flow, ERole role, [MarshalAs(UnmanagedType.LPWStr)] string defaultDeviceId);

        /// <summary>
        /// Property Value Changed
        /// </summary>
        /// <param name="pwstrDeviceId"></param>
        /// <param name="key"></param>
        void OnPropertyValueChanged([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId, PropertyKey key);
    }

    /// <summary>
    /// is defined in WTypes.h
    /// </summary>
    [Flags]
    enum ClsCtx
    {
        INPROC_SERVER = 0x1,
        INPROC_HANDLER = 0x2,
        LOCAL_SERVER = 0x4,
        INPROC_SERVER16 = 0x8,
        REMOTE_SERVER = 0x10,
        INPROC_HANDLER16 = 0x20,
        //RESERVED1	= 0x40,
        //RESERVED2	= 0x80,
        //RESERVED3	= 0x100,
        //RESERVED4	= 0x200,
        NO_CODE_DOWNLOAD = 0x400,
        //RESERVED5	= 0x800,
        NO_CUSTOM_MARSHAL = 0x1000,
        ENABLE_CODE_DOWNLOAD = 0x2000,
        NO_FAILURE_LOG = 0x4000,
        DISABLE_AAA = 0x8000,
        ENABLE_AAA = 0x10000,
        FROM_DEFAULT_CONTEXT = 0x20000,
        ACTIVATE_32_BIT_SERVER = 0x40000,
        ACTIVATE_64_BIT_SERVER = 0x80000,
        ENABLE_CLOAKING = 0x100000,
        PS_DLL = unchecked((int)0x80000000),
        INPROC = INPROC_SERVER | INPROC_HANDLER,
        SERVER = INPROC_SERVER | LOCAL_SERVER | REMOTE_SERVER,
        ALL = SERVER | INPROC_HANDLER
    }
    
    /// <summary>
    /// is defined in propsys.h
    /// </summary>
    [Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPropertyStore
    {
        int GetCount(out int propCount);
        int GetAt(int property, out PropertyKey key);
        int GetValue(ref PropertyKey key, out PropVariant value);
        int SetValue(ref PropertyKey key, ref PropVariant value);
        int Commit();
    }

    /// <summary>
    /// PROPERTYKEY is defined in wtypes.h
    /// </summary>
    public struct PropertyKey
    {
        /// <summary>
        /// Format ID
        /// </summary>
        public Guid formatId;
        /// <summary>
        /// Property ID
        /// </summary>
        public int propertyId;
        /// <summary>
        /// <param name="formatId"></param>
        /// <param name="propertyId"></param>
        /// </summary>
        public PropertyKey(Guid formatId, int propertyId)
        {
            this.formatId = formatId;
            this.propertyId = propertyId;
        }
    }

    public class FunctionDiscoveryKeys
    {
        public static PropertyKey FriendlyName = new PropertyKey(
            new Guid("{A45C254E-DF1C-4EFD-8020-67D146A850E0}"), 14);
    }
    
    [Flags]
    public enum DeviceState
    {
        /// <summary>
        /// DEVICE_STATE_ACTIVE
        /// </summary>
        Active = 0x00000001,
        /// <summary>
        /// DEVICE_STATE_DISABLED
        /// </summary>
        Disabled = 0x00000002,
        /// <summary>
        /// DEVICE_STATE_NOTPRESENT 
        /// </summary>
        NotPresent = 0x00000004,
        /// <summary>
        /// DEVICE_STATE_UNPLUGGED
        /// </summary>
        Unplugged = 0x00000008,
        /// <summary>
        /// DEVICE_STATEMASK_ALL
        /// </summary>
        All = 0x0000000F
    }

    public enum EDataFlow
    {
        /// <summary>
        /// Audio rendering stream. 
        /// Audio data flows from the application to the audio endpoint device, which renders the stream.
        /// </summary>
        eRender,
        /// <summary>
        /// Audio capture stream. Audio data flows from the audio endpoint device that captures the stream, 
        /// to the application
        /// </summary>
        eCapture,
        /// <summary>
        /// Audio rendering or capture stream. Audio data can flow either from the application to the audio 
        /// endpoint device, or from the audio endpoint device to the application.
        /// </summary>
        eAll
    };

    /// <summary>
    /// The ERole enumeration defines constants that indicate the role 
    /// that the system has assigned to an audio endpoint device
    /// </summary>
    public enum ERole
    {
        /// <summary>
        /// Games, system notification sounds, and voice commands.
        /// </summary>
        eConsole,

        /// <summary>
        /// Music, movies, narration, and live music recording
        /// </summary>
        eMultimedia,

        /// <summary>
        /// Voice communications (talking to another person).
        /// </summary>
        eCommunications,
    }

    /// <summary>
    /// MMDevice STGM enumeration
    /// </summary>
    enum StorageAccessMode
    {
        Read,
        Write,
        ReadWrite
    }

    /// <summary>
    /// from Propidl.h.
    /// http://msdn.microsoft.com/en-us/library/aa380072(VS.85).aspx
    /// contains a union so we have to do an explicit layout
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct PropVariant
    {
        [FieldOffset(0)]
        private short vt;
        [FieldOffset(2)]
        private short wReserved1;
        [FieldOffset(4)]
        private short wReserved2;
        [FieldOffset(6)]
        private short wReserved3;
        [FieldOffset(8)]
        private sbyte cVal;
        [FieldOffset(8)]
        private byte bVal;
        [FieldOffset(8)]
        private short iVal;
        [FieldOffset(8)]
        private ushort uiVal;
        [FieldOffset(8)]
        private int lVal;
        [FieldOffset(8)]
        private uint ulVal;
        [FieldOffset(8)]
        private int intVal;
        [FieldOffset(8)]
        private uint uintVal;
        [FieldOffset(8)]
        private long hVal;
        [FieldOffset(8)]
        private long uhVal;
        [FieldOffset(8)]
        private float fltVal;
        [FieldOffset(8)]
        private double dblVal;
        [FieldOffset(8)]
        private bool boolVal;
        [FieldOffset(8)]
        private int scode;
        //CY cyVal;
        [FieldOffset(8)]
        private DateTime date;
        [FieldOffset(8)]
        private System.Runtime.InteropServices.ComTypes.FILETIME filetime;
        //CLSID* puuid;
        //CLIPDATA* pclipdata;
        //BSTR bstrVal;
        //BSTRBLOB bstrblobVal;
        [FieldOffset(8)]
        private Blob blobVal;
        //LPSTR pszVal;
        [FieldOffset(8)]
        private IntPtr pointerValue; //LPWSTR 
        //IUnknown* punkVal;
        /*IDispatch* pdispVal;
        IStream* pStream;
        IStorage* pStorage;
        LPVERSIONEDSTREAM pVersionedStream;
        LPSAFEARRAY parray;
        CAC cac;
        CAUB caub;
        CAI cai;
        CAUI caui;
        CAL cal;
        CAUL caul;
        CAH cah;
        CAUH cauh;
        CAFLT caflt;
        CADBL cadbl;
        CABOOL cabool;
        CASCODE cascode;
        CACY cacy;
        CADATE cadate;
        CAFILETIME cafiletime;
        CACLSID cauuid;
        CACLIPDATA caclipdata;
        CABSTR cabstr;
        CABSTRBLOB cabstrblob;
        CALPSTR calpstr;
        CALPWSTR calpwstr;
        CAPROPVARIANT capropvar;
        CHAR* pcVal;
        UCHAR* pbVal;
        SHORT* piVal;
        USHORT* puiVal;
        LONG* plVal;
        ULONG* pulVal;
        INT* pintVal;
        UINT* puintVal;
        FLOAT* pfltVal;
        DOUBLE* pdblVal;
        VARIANT_BOOL* pboolVal;
        DECIMAL* pdecVal;
        SCODE* pscode;
        CY* pcyVal;
        DATE* pdate;
        BSTR* pbstrVal;
        IUnknown** ppunkVal;
        IDispatch** ppdispVal;
        LPSAFEARRAY* pparray;
        PROPVARIANT* pvarVal;
        */

        internal struct Blob
        {
            public int Length;
            public IntPtr Data;

            //Code Should Compile at warning level4 without any warnings, 
            //However this struct will give us Warning CS0649: Field [Fieldname] 
            //is never assigned to, and will always have its default value
            //You can disable CS0649 in the project options but that will disable
            //the warning for the whole project, it's a nice warning and we do want 
            //it in other places so we make a nice dummy function to keep the compiler
            //happy.
            private void FixCS0649()
            {
                Length = 0;
                Data = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Creates a new PropVariant containing a long value
        /// </summary>
        public static PropVariant FromLong(long value)
        {
            return new PropVariant() { vt = (short)VarEnum.VT_I8, hVal = value };
        }

        /// <summary>
        /// Helper method to gets blob data
        /// </summary>
        private byte[] GetBlob()
        {
            var blob = new byte[blobVal.Length];
            Marshal.Copy(blobVal.Data, blob, 0, blob.Length);
            return blob;
        }

        /// <summary>
        /// Interprets a blob as an array of structs
        /// </summary>
        public T[] GetBlobAsArrayOf<T>()
        {
            var blobByteLength = blobVal.Length;
            var singleInstance = (T)Activator.CreateInstance(typeof(T));
            var structSize = Marshal.SizeOf(singleInstance);
            if (blobByteLength % structSize != 0)
            {
                throw new InvalidDataException(String.Format("Blob size {0} not a multiple of struct size {1}", blobByteLength, structSize));
            }
            var items = blobByteLength / structSize;
            var array = new T[items];
            for (int n = 0; n < items; n++)
            {
                array[n] = (T)Activator.CreateInstance(typeof(T));
                Marshal.PtrToStructure(new IntPtr((long)blobVal.Data + n * structSize), array[n]);
            }
            return array;
        }

        /// <summary>
        /// Gets the type of data in this PropVariant
        /// </summary>
        public VarEnum DataType
        {
            get { return (VarEnum)vt; }
        }

        /// <summary>
        /// Property value
        /// </summary>
        public object Value
        {
            get
            {
                VarEnum ve = DataType;
                switch (ve)
                {
                    case VarEnum.VT_I1:
                        return bVal;
                    case VarEnum.VT_I2:
                        return iVal;
                    case VarEnum.VT_I4:
                        return lVal;
                    case VarEnum.VT_I8:
                        return hVal;
                    case VarEnum.VT_INT:
                        return iVal;
                    case VarEnum.VT_UI4:
                        return ulVal;
                    case VarEnum.VT_UI8:
                        return uhVal;
                    case VarEnum.VT_LPWSTR:
                        return Marshal.PtrToStringUni(pointerValue);
                    case VarEnum.VT_BLOB:
                    case VarEnum.VT_VECTOR | VarEnum.VT_UI1:
                        return GetBlob();
                    case VarEnum.VT_CLSID:
                        return (Guid)Marshal.PtrToStructure(pointerValue, typeof(Guid));
                }
                throw new NotImplementedException("PropVariant " + ve.ToString());
            }
        }

        /// <summary>
        /// allows freeing up memory, might turn this into a Dispose method?
        /// </summary>
        public void Clear()
        {
            PropVariantClear(ref this);
        }

        [DllImport("ole32.dll")]
        private static extern int PropVariantClear(ref PropVariant pvar);
    }

}

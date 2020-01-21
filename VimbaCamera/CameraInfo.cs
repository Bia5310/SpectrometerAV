using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AVT.VmbAPINET;

namespace VimbaCameraNET
{
    /// <summary>
    /// A simple container class for infos (name and ID) about a camera
    /// </summary>
    public class CameraInfo
    {
        /// <summary> The camera name</summary>
        private string m_Name = null;

        /// <summary> The camera ID</summary>
        private string m_ID = null;

        /// <summary>Camera interface id</summary>
        private string m_interface_id = null;

        /// <summary>Camera interface type</summary>
        private string m_interface_type = null;

        /// <summary>Camera serial</summary>
        private string m_serial_number = null;

        /// <summary>
        /// Initializes a new instance of the CameraInfo class.
        /// </summary>
        /// <param name="name">The camera name</param>
        /// <param name="id">The camera ID</param>
        public CameraInfo(string name, string id, string cam_interface, VmbInterfaceType interfaceType, string serialNumber)
        {
            if (null == name)
                throw new ArgumentNullException("name");

            if (null == id)
                throw new ArgumentNullException("id");

            if (cam_interface == null)
                throw new ArgumentNullException("camera interface");

            if (serialNumber == null)
                throw new ArgumentNullException("camera serial");

            this.m_Name = name;
            this.m_ID = id;
            this.m_interface_id = cam_interface;
            this.m_interface_type = interfaceType.ToString();
            this.m_serial_number = serialNumber;
        }

        public CameraInfo(Camera camera)
        {
            if (camera == null)
            {
                m_Name = "";
                m_ID = "";
                m_interface_id = "";
                m_serial_number = "";
                m_interface_type = InterfaceTypeToString(VmbInterfaceType.VmbInterfaceUnknown);
                return;
            }

            m_Name = camera.Name;
            m_ID = camera.Id;
            m_interface_id = camera.InterfaceID;
            m_interface_type = InterfaceTypeToString(camera.InterfaceType);
            m_serial_number = camera.SerialNumber;
        }

        /// <summary>
        /// Gets the name of the camera
        /// </summary>
        public string Name
        {
            get
            {
                return this.m_Name;
            }
        }

        /// <summary>
        /// Gets the ID
        /// </summary>
        public string ID
        {
            get
            {
                return this.m_ID;
            }
        }

        /// <summary>
        /// Gets the interface ID
        /// </summary>
        public string InterfaceID
        {
            get { return m_interface_id; }
        }

        /// <summary>
        /// Gets the interface type
        /// </summary>
        public string InterfaceType
        {
            get { return m_interface_type; }
        }

        /// <summary>
        /// Gets the serial number
        /// </summary>
        public string SerialNumber
        {
            get { return m_serial_number; }
        }

        /// <summary>
        /// Overrides the toString Method for the CameraInfo class (this)
        /// </summary>
        /// <returns>The Name of the camera</returns>
        public override string ToString()
        {
            return this.m_Name;
        }

        public static string InterfaceTypeToString(VmbInterfaceType interfaceType)
        {
            switch(interfaceType)
            {
                case VmbInterfaceType.VmbInterfaceEthernet:
                    return "Ethernet";
                case VmbInterfaceType.VmbInterfaceFirewire:
                    return "Fireware";
                case VmbInterfaceType.VmbInterfaceUsb:
                    return "USB";
                case VmbInterfaceType.VmbInterfaceCL:
                    return "CL";
            }
            return "Unknown";
        }
    }
}

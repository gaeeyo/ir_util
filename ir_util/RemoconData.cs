using System;
using USB_IR_Library;

namespace IrUtilApp {
    class RemoconData {
        public string Name { get; }
        public uint Freq { get; }
        public uint DataSize { get; }
        public byte[] Data { get; }

        public RemoconData(String name, uint freq, uint size, byte[] data) {
            Name = name;
            Freq = freq;
            DataSize = size;
            Data = data;
        }

        public void Execute() {
            int count = USBIR.openUSBIR_all();
            if (count > 0) {
                USBIR.writeUSBIRData_all(Freq, Data, DataSize);
                USBIR.closeUSBIR_all();
            }
        }
    }
}
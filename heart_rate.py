import asyncio
import socket
import sys
from bleak import BleakClient

# UDP Setup for Unity
UDP_IP = "127.0.0.1"  # Localhost (same PC)
UDP_PORT = 12345
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# BLE Configuration
HR_UUID = "00002a37-0000-1000-8000-00805f9b34fb"
DEVICE_MAC = "14:85:F4:D5:4C:97"  # Replace with your watch's MAC address

def send_hr_to_unity(hr):
    sock.sendto(str(hr).encode(), (UDP_IP, UDP_PORT))

def heart_rate_callback(sender, data):
    flags = data[0]
    if flags & 0x01:  # 16-bit HR value
        hr = (data[1] << 8) | data[2]
    else:             # 8-bit HR value
        hr = data[1]
    
    # Update the same line in the console
    sys.stdout.write(f"\rHeart Rate: {hr} BPM")
    sys.stdout.flush()
    
    # Send to Unity
    send_hr_to_unity(hr)

async def main():
    async with BleakClient(DEVICE_MAC) as client:
        await client.start_notify(HR_UUID, heart_rate_callback)
        while True:
            await asyncio.sleep(1)

asyncio.run(main())

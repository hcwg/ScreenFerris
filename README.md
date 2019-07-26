
# ScreenFerris

## Story
When we are using a tablet or your mobile phone, we usually read the text in a portrait mode, and play games or watch videos in landscape mode. Gravity sensor helps the devices to rotate its' screen automatically when we rotate the device, which improves the user experience. 

 
Similarly, when we use our PC and monitors, we still have different display needs when dealing with different things. For example, when we read text or read codes, we prefer to read in monitor portrait mode, and when we need a multi-window view, we prefer landscape mode. 

 
However, changing the display mode for monitors can be annoying. Each time, you need to click into the display setting and choose an orientation, then you can rotate the screen. What if we can make the monitors like the tablet or mobile phone, that can change its' display mode automatically if the monitor is rotated itself? 

**Yes, it can.** Using ScreenFerris along with Bluetooth Low Energy (BLE) acceleration sensor, you can have your monitor's content rotate automatically.

This project is a Microsoft Hackathon (Suzhou 2019) project. Hack for developers with love.

## Technology

ScrenFerris use [Accelerometer](https://en.wikipedia.org/wiki/Accelerometer) to measure the orientation of your monitor. And connect to the sensor(Accelerometer) using BLE. Then ScreenFerris WPF application can handle the calibration and connection of the sensor. Finally, ScreenFerris call windows API to rotate the content orientation of your PC.

## Features

- Support multiple monitors with multiple sensors at the same time.
- You can fix the sensor anywhere and in any attitude on your monitor. We have calibration.
- Can run in the background, only have a tray icon.
- Save sensor connection info, monitor-sensor binding info, to file and load them when start.

## Requirement

Hardware:
- Bluetooth LE adapter. Most laptop already has it. For desktop, you may need to buy one.
- Acceleration sensor.

If you want to buy such devices, please check our [supported devices](https://github.com/hcwg/ScreenFerris/wiki/Supported-devices)

Software:
- Windows 10 (For supporting of BLE API)

Build
- VisualStudio 2019
- .Net framework 4.6.2

# ScreenShot
![image](https://user-images.githubusercontent.com/1068203/61949304-fdf70100-afdc-11e9-8598-e8bd21f85a9b.png)

# Contributer

@YuzhenWANG @comzyh

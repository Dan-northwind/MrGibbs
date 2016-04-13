# MrGibbs

[![Build Status](https://travis-ci.org/brookpatten/MrGibbs.svg?branch=master)](https://travis-ci.org/brookpatten/MrGibbs)

[Project Website](http://mrgibbs.io/)

[Project Blog](http://blog.mrgibbs.io/)

[(bad) Circuit Diagrams](https://github.com/brookpatten/MrGibbs/tree/master/hw)

Configuration Options(forthcoming)

These instructions are intended to be used on Raspbian Jessie-Lite, although they should be comparable on any BlueZ5 equipped linux distro.

#1 Raspbian Setup
* Expand root partition
* Enable I2C (sudo raspi-config)
* Boot to command prompt

#2 Install BlueZ from source
* sudo apt-get update
* sudo apt-get install build-essential autoconf cmake libtool libglib2.0 libdbus-1-dev libudev-dev libical-dev libreadline-dev
* wget http://www.kernel.org/pub/linux/bluetooth/bluez-5.39.tar.xz
* tar xvf bluez-5.39.tar.xz 
* cd bluez-5.39/
* ./configure --prefix=/usr --mandir=/usr/share/man --sysconfdir=/etc --localstatedir=/var --enable-experimental --with-systemdsystemunitdir=/lib/systemd/system --with-systemduserunitdir=/usr/lib/systemd
* make
* sudo make install
* sudo systemctl daemon-reload
* sudo service bluetooth start
* sudo hciconfig hci0 up
* edit /etc/dbus-1/system.d/bluetooth.conf, add the following

```
<policy user="pi">
    <allow own="org.bluez"/>
    <allow send_destination="org.bluez"/>
    <allow send_interface="org.bluez.Agent1"/>
    <allow send_interface="org.bluez.MediaEndpoint1"/>
    <allow send_interface="org.bluez.MediaPlayer1"/>
    <allow send_interface="org.bluez.Profile1"/>
    <allow send_interface="org.freedesktop.DBus.ObjectManager"/>
</policy>
```
* update kernel
```
sudo apt-get install rpi-update
sudo rpi-update
```
* sudo reboot

#3 Install git
* sudo apt-get install git

#4A (Raspberry Pi 2) Mono Installation
As of this writing, a weekly build of mono is required as the necassary changes to mono.posix have not made it into a release yet.  CI builds do not include ArmHF packages so if you're intalling on a Pi, Weekly is the path of least resistance (Compiling mono from git on the pi is very time consuming).
* sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
* echo "deb http://download.mono-project.com/repo/debian nightly main" | sudo tee /etc/apt/sources.list.d/mono-nightly.list
* sudo apt-get update
* sudo apt-get install mono-snapshot-latest
* . mono-snapshot mono
 
#4B (Raspberry Pi A/B/+/Zero) Mono Compilation 
* git clone https://github.com/mono/mono.git
* sudo apt-get install autoconf libtool automake build-essential gettext
* cd mono
* ./autogen.sh --prefix=/usr/local
* make get-monolite-latest
* make (This will take about 4 hours)
* sudo make install
* cd ..

#5 clone Mr.Gibbs
* git clone --recursive git://github.com/brookpatten/MrGibbs.git

#6 Build
* cd MrGibbs/src
* xbuild

#7 Run It
* ./start.sh

#8 [Set it to run at boot](https://www.raspberrypi.org/documentation/linux/usage/rc-local.md)

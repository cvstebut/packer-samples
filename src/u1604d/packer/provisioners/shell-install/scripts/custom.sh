#!/bin/sh -eux

apt-get install -y xubuntu-desktop;

apt-get install -y xrdp;
systemctl enable xrdp;
usermod -aG ssl-cert xrdp

echo "xfce4-session" > /home/vagrant/.xsession
chown vagrant:vagrant /home/vagrant/.xsession

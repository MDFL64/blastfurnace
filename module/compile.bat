@echo off
g++ gclink.cpp -Isteamworks/public -shared -o ../program/gclink.dll -Lsteamworks/redistributable_bin -lsteam_api
pause
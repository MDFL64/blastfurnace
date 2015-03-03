@echo off
g++ gclink.cpp -Isteamworks/public -shared -static-libgcc -static-libstdc++ -o ../program/gclink.dll -Lsteamworks/redistributable_bin -lsteam_api
pause
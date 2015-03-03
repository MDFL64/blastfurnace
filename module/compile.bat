@echo off
g++ steamlink.cpp -Isteamworks/public -shared -o steamlink.dll -Lsteamworks/redistributable_bin -lsteam_api
pause
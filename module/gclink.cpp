#include <iostream>

#include "steam/steam_api.h"
#include "steam/isteamgamecoordinator.h"

#define DLL_FUNC __declspec(dllexport)

using namespace std;

extern "C" {
	ISteamGameCoordinator* DLL_FUNC _init() {
		if (!SteamAPI_Init()) {
			return NULL;
		}

		ISteamClient* client = SteamClient();

		HSteamUser hSteamUser = GetHSteamUser();
		HSteamPipe hSteamPipe = GetHSteamPipe();

		return reinterpret_cast<ISteamGameCoordinator*>(client->GetISteamGenericInterface(hSteamUser, hSteamPipe, STEAMGAMECOORDINATOR_INTERFACE_VERSION));
	}

	bool DLL_FUNC _check(ISteamGameCoordinator* gc, uint32* msg_size) {
		return gc->IsMessageAvailable(msg_size);
	}

	bool DLL_FUNC _recv(ISteamGameCoordinator* gc, uint32 msg_size, uint32* msg_type, char* buffer) {
		EGCResults res = gc->RetrieveMessage(msg_type, buffer, msg_size, &msg_size);

		if (res != k_EGCResultOK)
			return false;

		return true;
	}

	bool DLL_FUNC _send(ISteamGameCoordinator* gc, uint32 msg_type, char* buffer, uint32 buffer_len) {
		EGCResults res = gc->SendMessage(msg_type, buffer, buffer_len);

		if (res != k_EGCResultOK)
			return false;

		return true;
	}
}

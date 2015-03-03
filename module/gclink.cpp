// BlastFurnace.cpp : Defines the entry point for the console application.
//

//#define VERSION_SAFE_STEAM_API_INTERFACES

/*
#include <cstdint>

#include <iostream>
#include "steam/steam_api.h"

#include "steam/isteamgamecoordinator.h"

using namespace std;

struct wire {
	int type;
	int id;
};

class ProtoReader {
public:
	ProtoReader(uint8_t* buffer,uint32_t length);
	~ProtoReader();

	uint64_t readVarInt();
	uint64_t read64();

	wire readWire();
private:
	uint32_t length;
	uint8_t* buffer;

	uint32_t cursor = 0;
};

ProtoReader::ProtoReader(uint8_t* buffer,uint32_t length) {
	this->buffer = buffer;
	this->length = length;
}

ProtoReader::~ProtoReader() {
	delete buffer;
}

uint64_t ProtoReader::readVarInt() {
	uint64_t result=0;
	int offset=0;

	for (;;) {
		uint64_t b = buffer[cursor++];
		
		result |= ((b & 0x7F) << offset);

		if ((b & 0x80) == 0)
			return result;

		offset+=7;
	}
}

uint64_t ProtoReader::read64() {
	cursor+=8;
	return 0;
}

wire ProtoReader::readWire() {
	uint64_t result = readVarInt();

	wire w;
	w.type = result & 7;
	w.id = result >> 3;
	return w;
}

void pause() {
	cout << "Press any key to continue." << endl;
	getchar();
}

int main(int argc, char* argv[])
{
	cout << "Connecting to Steam..." << endl;
	if (!SteamAPI_Init()) {
	//if (!SteamAPI_InitSafe()) {
		cout << "Failed to connect to Steam." << endl;
		pause();
		return 1;
	}
	cout << "Steam ready!" << endl;

	ISteamUser* user = SteamUser();
	ISteamFriends* friends = SteamFriends();

	ISteamClient* client = SteamClient();
	
	HSteamUser hSteamUser = GetHSteamUser();
	HSteamPipe hSteamPipe = GetHSteamPipe();
	
	ISteamGameCoordinator* gc = reinterpret_cast<ISteamGameCoordinator*>(client->GetISteamGenericInterface(hSteamUser, hSteamPipe, STEAMGAMECOORDINATOR_INTERFACE_VERSION));

	const char* name = friends->GetPersonaName();

	cout << "Hello, " << name << "!" << endl;

	uint32_t msg_size;

	while (true) {
		if (gc->IsMessageAvailable(&msg_size)) {
			uint32_t msg_type;
			uint8_t* msg_payload = new uint8_t[msg_size];
			EGCResults res = gc->RetrieveMessage(&msg_type, msg_payload, msg_size, &msg_size);
			if (res != k_EGCResultOK) {
				delete msg_payload;
				continue;
			}

			ProtoReader reader(msg_payload, msg_size);

			msg_type &= 0x7FFFFFFF;
			
			if (msg_type == 1049) {
				for (int i=0;i<3;i++) {
					wire w = reader.readWire();

					cout << "id = " << w.id << endl;
					cout << "type = " << w.type << endl;

					if (w.type==0) {
						reader.readVarInt();
						cout << "(varint)" << endl;
					} else if (w.type==1) {
						reader.read64();
						cout << "(64bit)" << endl;
					} else {
						cout << "(unknown)" << endl;
						break;
					}
				}

				break;
			} else {
				cout << "GOOD: " << msg_type << " : " << msg_size << endl;
			}
		}
	}

	pause();
	return 0;
}
*/

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
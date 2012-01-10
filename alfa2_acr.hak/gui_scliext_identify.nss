//
// gui_scliext_identify
//
// This support script lets DMs using the client extension view creatures in
// areas other than their current area.
//

#include "acr_scliext_i"

void main( int nFlagsAndProtocolVersion, int nClientExtensionVersion )
{
	object oCallerPC          = OBJECT_SELF;
	int    nSupportedFeatures = ACR_SCLIEXT_DEFAULT_FEATURE_BITS;

	// Remember the CE version and send a nag message to the user if they have
	// an older version.
	SetLocalInt(OBJECT_SELF, ACR_SCLIEXT_VERSION, nClientExtensionVersion);
	SetLocalInt(OBJECT_SELF, ACR_SCLIEXT_FEATURES, nFlagsAndProtocolVersion);

	if (nClientExtensionVersion < ACR_MakeClientExtensionVersion(ACR_SCLIEXT_LATEST_VERSION))
	{
		DelayCommand(15.0f,
			SendMessageToPC(OBJECT_SELF, "You appear to be running an older version of the Client Extension, and a newer version is available (recommended).  View http://www.alandfaraway.org/forums/viewtopic.php?f=231&t=43769 (ALFA Forums, ALFA General, New Players, Skywing's NWN2 Client Extension) for details on how to upgrade."));
	}

	//
	// Tell the client that we support area polling.
	//

	SendMessageToPC(
		oCallerPC,
		"SCliExt10" + GetStringRight(
			IntToHexString(
				nSupportedFeatures ),
			8 )
		);
}


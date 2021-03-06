////////////////////////////////////////////////////////////////////////////////
//
//  System Name : ALFA Core Rules
//     Filename : acr_cs_atcheck
//    $Revision:: 153         $ current version of the file
//        $Date:: 2006-10-09 #$ date the file was created or modified
//       Author : � i p h � r
//
//    Var Prefix: ACR_CST
//  Dependencies: None
//
//  Description
//  This file implements a generic attribute check function to be used in conversations.
//
//  Revision History
//  2006/10/05  � i p h � r  Inception
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
// Includes ////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
// Constants ///////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
// Structures //////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
// Function Prototypes /////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
// Function Definitions ////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

int StartingConditional(int nAbility, int nValue, int bBaseScore)
{
    object oPC = GetPCSpeaker();

    // return TRUE or FALSE depending on the player's ability score
    return (GetAbilityScore(oPC, nAbility, bBaseScore) >= nValue) ? TRUE : FALSE;
}

//::///////////////////////////////////////////////
//:: Invocation: Eldritch Spear (Warlock Spelllike effect)
//:: NW_S0_IEldSpear.nss
//:://////////////////////////////////////////////
/*
    Does 1d6 Dmg per "ranking" of Eldritch Blast.
    Longer range than normal Eldritch Blast.
*/
//:://////////////////////////////////////////////
//:: Created By: Jesse Reynolds (JLR - OEI)
//:: Created On: June 20, 2005
//:://////////////////////////////////////////////
//:: VFX Pass By: Preston W, On: June 20, 2001


#include "nw_i0_invocatns"
#include "x2_inc_spellhook" 

void main()
{

/* 
  Spellcast Hook Code 
  Added 2003-06-23 by GeorgZ
  If you want to make changes to all spells,
  check x2_inc_spellhook.nss to find out more
  
*/

    if (!X2PreSpellCastCode())
    {
	// If code within the PreSpellCastHook (i.e. UMD) reports FALSE, do not run this spell
        return;
    }

// End of Spell Cast Hook

    DoShapeEldritchSpear();
}
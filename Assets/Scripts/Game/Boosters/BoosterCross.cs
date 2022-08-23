using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// "Cut" booster
// This object must be in the UI-panel of the booster. During activation (OnEnable) it turn a special mode of interaction with chips (ControlAssistant ignored)
[RequireComponent (typeof (BoosterButton))]
public class BoosterCross : IBoosterLogic {

    public Animation spoon;

	// Coroutine of special control mode
	public override IEnumerator Logic () {
        spoon.gameObject.SetActive(false);

		yield return StartCoroutine (Utils.WaitFor (Session.Instance.CanIWait, 0.1f));

		Slot target = null;
		while (true) {
			if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
				target = FieldControl.Instance.GetSlotFromTouch();
            if (target != null && (!target.chip ||  target.chip.chipType != "Key")) {

                spoon.transform.position = target.transform.position;
                spoon.gameObject.SetActive(true);
                spoon.Play();

                //CPanel.uiAnimation++;

                yield return new WaitForSeconds(0.91f);

                if(PlayerManager.Instance && !PlayerManager.Instance.m_infinityTools)
                    PlayerManager.Instance.m_boosterCrossCount--;
                //ItemCounter.RefreshAll();
               
                FieldManager.Instance.StoneCrush(target.coord);
				FieldManager.Instance.BlockCrush(target.coord, false);
				
                

                Chip lchip = null;

                if (target.chip) {
                    lchip = FieldManager.Instance.AddPowerup(target.coord, "CrossBomb");
  
                   // target.chip.DestroyChip();
                }

                Session.Instance.EventCounter();

                while (spoon.isPlaying)
                    yield return 0;

                yield return new WaitForSeconds(0.1f);

                if (lchip != null)
                {
                    ((CrossBomb)lchip.logic).LineMix(lchip);
                   // target.chip.DestroyChip();
                }

                spoon.gameObject.SetActive(false);
                Destroy(gameObject);
                //CPanel.uiAnimation--;

                break;
			}
			yield return 0;
		}

        this.Disable();
        //UIAssistant.main.ShowPage("Field");
	}
}

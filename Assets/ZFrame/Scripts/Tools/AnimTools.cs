using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimTools
{
	public static AnimatorStateInfo GetStateInfo(this Animator self, int layerIndex)
	{
		return self.IsInTransition(layerIndex) ? 
			       self.GetNextAnimatorStateInfo(layerIndex) :
			       self.GetCurrentAnimatorStateInfo(layerIndex);
	}

	public static bool HasState(this Animator self, int stateId)
	{
		for (int i = 0; i < self.layerCount; ++i) {
			if (self.HasState(i, stateId)) return true;
		}

		return false;
	}

    public static bool HasParamater(this Animator self, int nameHash)
    {
        for (int i = 0; i < self.parameterCount; ++i) {
            var param = self.GetParameter(i);
            if (param.nameHash == nameHash) return true;
        }

        return false;
    }

    public static void ResetParamaters(this Animator self)
    {
        for (int i = 0; i < self.parameterCount; ++i) {
            var param = self.GetParameter(i);
            switch (param.type) {
                case AnimatorControllerParameterType.Bool:
                    self.SetBool(param.nameHash, param.defaultBool);
                    break;
                case AnimatorControllerParameterType.Float:
                    self.SetFloat(param.nameHash, param.defaultFloat);
                    break;
                case AnimatorControllerParameterType.Int:
                    self.SetInteger(param.nameHash, param.defaultInt);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    self.ResetTrigger(param.nameHash);
                    break;
            }
        }
    }

	public static void PlayInitState(this Animator self, int stateHash)
	{
		for (int i = 0; i < self.layerCount;++i) {
			if (self.HasState(i, stateHash)) {
				self.Play(stateHash, i);
			}
		}
	}
	
}

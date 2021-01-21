using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;

namespace AdditionalMaps.Utils
{
	public static class FsmUtil
	{
		public static void AddEventTransition(this PlayMakerFSM fsm, string stateName, string eventName, string toState)
		{
			FsmState state = fsm.Fsm.GetState(stateName);
			List<FsmTransition> list = state.Transitions.ToList();
			list.Add(new FsmTransition
			{
				ToState = toState,
				FsmEvent = (FsmEvent.FindEvent(eventName) == null) ? new FsmEvent(eventName) : FsmEvent.FindEvent(eventName)
			});
			state.Transitions = list.ToArray();
		}
	}
}

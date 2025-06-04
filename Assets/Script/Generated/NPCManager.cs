using System;
using UnityEngine;
using System.Collections.Generic;

// Attached to NPC GameObjects to handle player interaction, dialogue, and quests.
public class NPCManager : MonoBehaviour
{
    [Header("NPC Identification")]
    [Tooltip("Optional unique ID for this specific NPC.")]
    [SerializeField] private string npcID = "generic_npc";
    [Tooltip("Display name shown in UI.")]
    [SerializeField] private string npcName = "Mysterious Stranger";

    [Header("Dialogue")]
    [Tooltip("Default dialogue to display when interacted with.")]
    [TextArea][SerializeField] private string defaultDialogue = "Hello there, traveler.";
    // Potential future: Link to a Dialogue Tree asset instead of single string

    [Header("Quests Offered")]
    [Tooltip("List of Quest IDs this NPC can offer.")]
    [SerializeField] private List<string> questsToOffer = new List<string>();

    [Header("Quests Completed Here")]
    [Tooltip("List of Quest IDs that should be turned in to this NPC.")]
    [SerializeField] private List<string> questsToComplete = new List<string>();

    // Removed static events - Interaction is instance-based.
    // Instead, interaction will trigger methods on other managers (Dialogue, Quest).

    // --- Interaction Logic ---

    /// <summary>
    /// Called by an interaction system (e.g., PlayerController detects proximity and key press).
    /// Initiates the interaction flow with this NPC.
    /// </summary>
    public void Interact()
    {
        Debug.Log($"Player interacted with {npcName} ({npcID}).");

        // --- Determine NPC State ---
        // 1. Check if player needs to turn in a quest TO THIS NPC.
        string turnInQuestID = FindReadyTurnInQuest();
        if (!string.IsNullOrEmpty(turnInQuestID))
        {
            // Start turn-in dialogue / process
            StartQuestTurnInSequence(turnInQuestID);
            return; // Handle turn-in first
        }

        // 2. Check if NPC has a NEW quest to offer that player meets requirements for.
        string offerQuestID = FindAvailableQuestToOffer();
        if (!string.IsNullOrEmpty(offerQuestID))
        {
            // Start quest offering dialogue / process
            StartQuestOfferSequence(offerQuestID);
            return;
        }

        // 3. Check if player is currently ON a quest involving this NPC (e.g., objective = Interact with npcID)
        string relatedActiveQuestID = FindRelatedActiveQuest();
        if (!string.IsNullOrEmpty(relatedActiveQuestID)) {
            // Start dialogue related to the active quest objective
            StartRelatedQuestDialogue(relatedActiveQuestID);
             // Notify QuestManager that interaction objective is met
            QuestManager.Instance?.NotifyProgress(QuestObjective.ObjectiveType.Interact, npcID);
            return;
        }


        // 4. If none of the above, show default dialogue.
        ShowDialogue(defaultDialogue);
    }

    // --- Helper Methods ---

    private string FindReadyTurnInQuest()
    {
        if (QuestManager.Instance == null) return null;
        foreach (string questID in questsToComplete)
        {
            if (QuestManager.Instance.IsQuestReadyToTurnIn(questID))
            {
                return questID; // Found a quest ready for turn-in
            }
        }
        return null; // No quests ready to turn in here
    }

    private string FindAvailableQuestToOffer()
    {
        if (QuestManager.Instance == null || CharacterManager.Instance?.character == null) return null;

        Character character = CharacterManager.Instance.character;

        foreach (string questID in questsToOffer)
        {
             Quest.QuestStatus status = QuestManager.Instance.GetQuestStatus(questID);
             // Check if quest is NotStarted (or maybe Failed, if repeatable?)
            if (status == Quest.QuestStatus.NotStarted)
            {
                 // Check requirements (Level, Prereqs) from the database version
                Quest questDef = QuestManager.Instance.GetQuestDatabase()?.GetQuestByID(questID);
                if (questDef != null && character.Level >= questDef.requiredLevel)
                {
                     // Optional: Check prerequisite quests from QuestManager
                     // if (string.IsNullOrEmpty(questDef.prerequisiteQuestID) || QuestManager.Instance.IsQuestCompleted(questDef.prerequisiteQuestID)) {
                         return questID; // Found an available quest
                     // }
                }
            }
        }
        return null; // No new quests available
    }

    private string FindRelatedActiveQuest() {
        if (QuestManager.Instance == null) return null;
        List<Quest> activeQuests = QuestManager.Instance.GetActiveQuests();
        foreach(Quest q in activeQuests) {
            foreach(QuestObjective obj in q.objectives) {
                // Check if an objective requires interacting with this specific NPC
                if(obj.type == QuestObjective.ObjectiveType.Interact && obj.targetID == this.npcID && !q.IsObjectiveComplete(q.objectives.IndexOf(obj))) {
                    return q.questID; // Found active quest objective involving this NPC
                }
            }
        }
        return null;
    }

    // --- Sequence Starters (Placeholders - Need Dialogue System) ---

    private void ShowDialogue(string dialogueText)
    {
        Debug.Log($"{npcName}: \"{dialogueText}\"");
        // --- Actual Implementation ---
        // DialogueUIManager.Instance.ShowDialogue(npcName, dialogueText, options);
    }

    private void StartQuestOfferSequence(string questID)
    {
        Quest questDef = QuestManager.Instance?.GetQuestDatabase()?.GetQuestByID(questID);
        if (questDef == null) return;

        // Example Dialogue Flow:
        string offerDialogue = $"I have a task for you, traveler. It's called '{questDef.questName}'. {questDef.questDescription}. Will you accept?";
        Debug.Log($"{npcName}: \"{offerDialogue}\"");

        // --- Actual Implementation ---
        // List<DialogueOption> options = new List<DialogueOption>();
        // options.Add(new DialogueOption("Accept", () => {
        //     if(QuestManager.Instance.AcceptQuest(questID)) {
        //          ShowDialogue("Excellent! Get started right away.");
        //     } else {
        //          ShowDialogue("Hmm, seems there was an issue. Never mind.");
        //     }
        // }));
        // options.Add(new DialogueOption("Decline", () => {
        //     ShowDialogue("Perhaps another time then.");
        // }));
        // DialogueUIManager.Instance.ShowDialogue(npcName, offerDialogue, options);
    }

     private void StartQuestTurnInSequence(string questID)
    {
        Quest questInstance = QuestManager.Instance?.GetPlayerQuests()[questID]; // Assumes ID exists if FindReady worked
        if (questInstance == null) return;

         string turnInDialogue = $"Ah, you've returned! Have you completed the task '{questInstance.questName}'?";
         Debug.Log($"{npcName}: \"{turnInDialogue}\"");

         // --- Actual Implementation ---
         // List<DialogueOption> options = new List<DialogueOption>();
         // options.Add(new DialogueOption("Complete Quest", () => {
         //    if(QuestManager.Instance.TurnInQuest(questID)) {
         //         ShowDialogue("Well done! Here is your reward.");
         //    } else {
         //         ShowDialogue("Something seems amiss... Are you sure you finished everything?");
         //    }
         // }));
         // options.Add(new DialogueOption("Not Yet", () => {
         //     ShowDialogue("Keep at it, then.");
         // }));
         // DialogueUIManager.Instance.ShowDialogue(npcName, turnInDialogue, options);
    }

     private void StartRelatedQuestDialogue(string questID)
     {
         Quest questInstance = QuestManager.Instance?.GetPlayerQuests()[questID];
         if (questInstance == null) return;
         // Find the specific objective related to this NPC... (more complex lookup needed)
         string relatedDialogue = $"Ah, you're working on '{questInstance.questName}' for me? Good. Keep it up!"; // Example generic line
         ShowDialogue(relatedDialogue);
     }

}
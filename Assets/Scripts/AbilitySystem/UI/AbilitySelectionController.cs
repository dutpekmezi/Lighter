using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace dutpekmezi
{
    public class AbilitySelectionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AbilityCard abilityCardPrefab;
        [SerializeField] private Transform abilityCardsParent;

        [Header("Settings")]
        [SerializeField] private int maxDisplayingCardsCount = 3;

        private List<AbilityCard> displayingAbilityCards = new List<AbilityCard>();

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                DisplayAbilityCards();
            }
        }
        public void DisplayAbilityCards()
        {
            abilityCardsParent.gameObject.SetActive(true);

            RemoveCards(); // if displaying cards count greater than maxDisplayingCardsCount remove cards until desired maxDisplayingCardsCount

            var abilities = AbilitySystem.Instance.Abilities;

            for (int i = 0; i < maxDisplayingCardsCount; i++)
            {
                if (displayingAbilityCards.Count < maxDisplayingCardsCount)
                {
                    AbilityCard card = Instantiate(abilityCardPrefab, abilityCardsParent);
                    displayingAbilityCards.Add(card);
                }

                if (i < abilities.Count && i < displayingAbilityCards.Count)
                {
                    displayingAbilityCards[i].Init(abilities[i]);
                }
            }
        }

        public void HideAbilityCards()
        {
            abilityCardsParent.gameObject.SetActive(false);
        }

        private void RemoveCards()
        {
            if (displayingAbilityCards.Count <= maxDisplayingCardsCount) return;

            while (displayingAbilityCards.Count > maxDisplayingCardsCount)
            {
                Destroy(displayingAbilityCards[maxDisplayingCardsCount].gameObject);
                displayingAbilityCards.RemoveAt(maxDisplayingCardsCount);
            }
        }
    }
}
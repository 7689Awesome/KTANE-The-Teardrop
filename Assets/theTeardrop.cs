using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class theTeardrop : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;

   public String[] possibleWords;

   public String[] condition1Names;
   public String[] condition2Names;
   public String[] condition3Names;
   public String[] condition4Names;
   public String[] otherwiseNames;

   public int[] personCryingInNumbers;
   public int[] correctWordInNumbers;

   public TextMesh displayTextMesh;

   public List <int> serialNumberNumbers = new List <int>();

   public KMSelectable Teardrop;
   
   public string personCrying;
   public string firstIndicator;
   public string correctWord;
   public string encryptedWord;

   public string originalAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
   public string correctString = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

   public int sumOfSerialNumberDigits = 0;
   public int numberOfOnIndicators = 0;
   public int numberOfOffIndicators = 0;
   public int numberOfModules = 0;
   public int numberOfSerialNumberLetters = 0;
   public int matchesBetweenSerialNumberAndTEARDROPS = 0;
   public int oddIndexToMove = 0;
   public int evenIndexToMove = 0;
   public int finalAnswer = 0;
   public int sumOfLetterValuesOfPersonCrying = 0;
   public int sumOfLetterValuesOfDecryptedWord = 0;
   public int stage = 0;


   public int halfLength;
   public string secondHalf;
   public string firstHalf;

   public string serialNumber;

   private string withoutPrimeLetters;
   private string withoutCRYLetters;

   static int ModuleIdCounter = 1;
   int ModuleId;
   private bool ModuleSolved;

   void Awake () { //Avoid doing calculations in here regarding edgework. Just use this for setting up buttons for simplicity.
      ModuleId = ModuleIdCounter++;
      Teardrop.OnInteract += delegate () { TeardropPressed(); return false; };
   }

   void Start () { //Shit that you calculate, usually a majority if not all of the module
      CorrectWordChooser();
      EncryptWord();
      GetFirstIndicator();
      DeterminePersonCrying();
      FinalAnswer();
   }

   void CorrectWordChooser(){ 
      int possibleWordsIndex = Rnd.Range(0, 40);
      correctWord = possibleWords[possibleWordsIndex].ToUpper();
      Debug.LogFormat("[The Teardrop #{0}] The decrypted word should be: {1}", ModuleId, correctWord);
   }

   bool CalcIsPrime(int number) {

    if (number == 1) return false;
    if (number == 2) return true;
    if (number % 2 == 0) return false; // Even number     

    for (int i = 2; i <= Math.Sqrt(number); i++) { // Check up to the square root
       if (number % i == 0) return false;
    }
    return true;
}
   void EncryptWord(){
      foreach (var a in Bomb.GetSerialNumberNumbers()){
         sumOfSerialNumberDigits += a;
      }

      Debug.LogFormat("[The Teardrop #{0}] Starting string: {1}", ModuleId, originalAlphabet);
      if((Bomb.GetBatteryCount()*Bomb.GetPortCount()) % 2 == 1){
         correctString = "ZYXWVUTSRQPONMLKJIHGFEDCBA";
         Debug.LogFormat("[The Teardrop #{0}] Since the product of the number of batteries and number of ports is odd, the string must be reversed.", ModuleId);
         Debug.LogFormat("[The Teardrop #{0}] Current string: {1}", ModuleId, correctString);
      }

      Debug.Log(CalcIsPrime(sumOfSerialNumberDigits));
      if(CalcIsPrime(sumOfSerialNumberDigits)){
         withoutPrimeLetters = correctString.Replace("P", "").Replace("R", "").Replace("I", "").Replace("M", "").Replace("E", "");
         correctString = "PRIME" + withoutPrimeLetters;

         Debug.LogFormat("[The Teardrop #{0}] Since the sum of the digits in the serial number is prime, the letters in the substring PRIME must be removed from the current string, then added as a whole at the beginning.", ModuleId);
         Debug.LogFormat("[The Teardrop #{0}] Current string: {1}", ModuleId, correctString);
        }

         foreach (var letter in Bomb.GetSerialNumberLetters()){
            numberOfSerialNumberLetters++;
         }

      if(Bomb.GetBatteryCount()>numberOfSerialNumberLetters){
         halfLength = correctString.Length / 2;
         // Take the second half of the string
         secondHalf = correctString.Substring(halfLength);
         // Take the first half of the string
         firstHalf = correctString.Substring(0, halfLength);
         // Move the second half in front of the first half
         correctString = secondHalf + firstHalf;
         Debug.LogFormat("[The Teardrop #{0}] Since the number of batteries is greater than the number of letters in the serial number, the second half of the string must be cut and moved to the front.", ModuleId);
         Debug.LogFormat("[The Teardrop #{0}] Current string: {1}", ModuleId, correctString);
        }

      serialNumber = Bomb.GetSerialNumber();

      foreach (char character in "TEARDROP")
         {
               if (serialNumber.Contains(character))
               {
                  matchesBetweenSerialNumberAndTEARDROPS++;
               }
         }

      if (matchesBetweenSerialNumberAndTEARDROPS >= 1) {
         withoutCRYLetters = correctString.Replace("C", "").Replace("R", "").Replace("Y", "");
         correctString = withoutCRYLetters + "CRY";
         Debug.LogFormat("[The Teardrop #{0}] Since the Serial Number contains at least one character in the word TEARDROP, CRY must be extracted, then appended.", ModuleId);
         Debug.LogFormat("[The Teardrop #{0}] Current string: {1}", ModuleId, correctString);
        }

      foreach (var module in Bomb.GetModuleNames()){
         numberOfModules++;
      }

      if (numberOfModules % 2 == 0){
         evenIndexToMove = Bomb.GetSerialNumberNumbers().Last()-1;

         correctString = MoveCharacterToEnd(correctString, evenIndexToMove);

         Debug.LogFormat("[The Teardrop #{0}] Since the total number of modules is even, the letter in the position of the last digit of the serial number must be extracted, then appended.", ModuleId);
         Debug.LogFormat("[The Teardrop #{0}] Current string: {1}", ModuleId, correctString);
        }

      if (numberOfModules % 2 == 1){
         oddIndexToMove = Bomb.GetSerialNumberNumbers().First()-1;

         correctString = MoveCharacterToEnd(correctString, oddIndexToMove);

         Debug.LogFormat("[The Teardrop #{0}] Since the total number of modules is odd, the letter in the position of the first digit of the serial number must be extracted, then appended.", ModuleId);
         Debug.LogFormat("[The Teardrop #{0}] Current string: {1}", ModuleId, correctString);
      }

      if ((Bomb.GetPortCount(Port.Parallel) == 1) && (Bomb.IsIndicatorOn("BOB"))){
         correctString = originalAlphabet;
         Debug.LogFormat("[The Teardrop #{0}] Since there is one parallel port and a lit BOB indicator is present, all previous rules no longer apply.", ModuleId);
         Debug.LogFormat("[The Teardrop #{0}] Current string: {1}", ModuleId, correctString);
      }

      Debug.LogFormat("[The Teardrop #{0}] Therefore, Final String: {1}", ModuleId, correctString);

      //========================================================================//

      int[] indicesArray = new int[correctWord.Length];
      
      foreach(char x in correctWord){
         indicesArray[stage] = correctString.IndexOf(x);
         encryptedWord += originalAlphabet[indicesArray[stage]];
         stage++;
      }

      Debug.LogFormat("[The Teardrop #{0}] Thus, the encrypted word is {1}", ModuleId, encryptedWord);
      
      displayTextMesh.text = encryptedWord;

   }
//==================================================================================
   string MoveCharacterToEnd(string input, int index)
    {
        if (index >= 0 && index < input.Length - 1)
        {
            char charToMove = input[index];
            string modifiedString = input.Remove(index, 1);
            modifiedString += charToMove;
            return modifiedString;
        }

        return input;
    }
   
   static int[] ConvertLettersToAlphabetPositions(string input)
    {
        int[] result = new int[input.Length];
        int index = 0;

        foreach (char c in input)
        {
            if (char.IsLetter(c))
            {
                char upperC = char.ToUpper(c);
                int position = upperC - 'A' + 1;
                result[index++] = position;
            }
            else
            {
                // If the character is not a letter, store a placeholder value (e.g., 0)
                result[index++] = 0;
            }
        }

        // Resize the array to the actual number of letter positions
        Array.Resize(ref result, index);
        return result;
    }

   void GetFirstIndicator(){
      string[] sortedIndicators = Bomb.GetIndicators().OrderBy(s => s).ToArray();
      
      if ((sortedIndicators != null) && (sortedIndicators.Length > 0)){
         firstIndicator = sortedIndicators[0];
      } else {
         firstIndicator = "none";
         Debug.Log("There are no indicators.");
      }
   }

   void DeterminePersonCrying(){
      Debug.Log(sumOfSerialNumberDigits);

      foreach (var b in Bomb.GetOnIndicators()){
         numberOfOnIndicators += 1;
      }

      foreach (var b in Bomb.GetOffIndicators()){
         numberOfOffIndicators += 1;
      }

      if (Bomb.GetPortCount() % 2 == 0){
         Debug.LogFormat("[The Teardrop #{0}] Since the number of ports is even, the first column should be used.", ModuleId);
         Debug.LogFormat("[The Teardrop #{0}] The first indicator in alphabetical order is {1}.", ModuleId, firstIndicator);
         if (firstIndicator == "BOB") {
            personCrying = condition1Names[0];
         } else if (firstIndicator == "CAR") {
            personCrying = condition1Names[1];
         } else if (firstIndicator == "CLR") {
            personCrying = condition1Names[2];
         } else if (firstIndicator == "FRK") {
            personCrying = condition1Names[3];
         } else if (firstIndicator == "FRQ") {
            personCrying = condition1Names[4];
         } else if (firstIndicator == "IND") {
            personCrying = condition1Names[5];
         } else if (firstIndicator == "MSA") {
            personCrying = condition1Names[6];
         } else if (firstIndicator == "NSA") {
            personCrying = condition1Names[7];
         } else if (firstIndicator == "SIG") {
            personCrying = condition1Names[8];
         } else if (firstIndicator == "SND") {
            personCrying = condition1Names[9];
         } else if (firstIndicator == "TRN") {
            personCrying = condition1Names[10];
         } else {
            personCrying = condition1Names[11];
         }

      } else if (Bomb.GetBatteryCount()%2 == 1){
         Debug.LogFormat("[The Teardrop #{0}] Since the number of batteries is odd, the second column should be used.", ModuleId);
            Debug.LogFormat("[The Teardrop #{0}] The first indicator in alphabetical order is {1}.", ModuleId, firstIndicator);
            if (firstIndicator == "BOB") {
            personCrying = condition2Names[0];
         } else if (firstIndicator == "CAR") {
            personCrying = condition2Names[1];
         } else if (firstIndicator == "CLR") {
            personCrying = condition2Names[2];
         } else if (firstIndicator == "FRK") {
            personCrying = condition2Names[3];
         } else if (firstIndicator == "FRQ") {
            personCrying = condition2Names[4];
         } else if (firstIndicator == "IND") {
            personCrying = condition2Names[5];
         } else if (firstIndicator == "MSA") {
            personCrying = condition2Names[6];
         } else if (firstIndicator == "NSA") {
            personCrying = condition2Names[7];
         } else if (firstIndicator == "SIG") {
            personCrying = condition2Names[8];
         } else if (firstIndicator == "SND") {
            personCrying = condition2Names[9];
         } else if (firstIndicator == "TRN") {
            personCrying = condition2Names[10];
         } else {
            personCrying = condition2Names[11];
         }
      } else if (CalcIsPrime(sumOfSerialNumberDigits)){
         Debug.LogFormat("[The Teardrop #{0}] Since the sum of the digits in the serial number is composite, the third column should be used.", ModuleId);
            Debug.LogFormat("[The Teardrop #{0}] The first indicator in alphabetical order is {1}.", ModuleId, firstIndicator);
            if (firstIndicator == "BOB") {
            personCrying = condition3Names[0];
         } else if (firstIndicator == "CAR") {
            personCrying = condition3Names[1];
         } else if (firstIndicator == "CLR") {
            personCrying = condition3Names[2];
         } else if (firstIndicator == "FRK") {
            personCrying = condition3Names[3];
         } else if (firstIndicator == "FRQ") {
            personCrying = condition3Names[4];
         } else if (firstIndicator == "IND") {
            personCrying = condition3Names[5];
         } else if (firstIndicator == "MSA") {
            personCrying = condition3Names[6];
         } else if (firstIndicator == "NSA") {
            personCrying = condition3Names[7];
         } else if (firstIndicator == "SIG") {
            personCrying = condition3Names[8];
         } else if (firstIndicator == "SND") {
            personCrying = condition3Names[9];
         } else if (firstIndicator == "TRN") {
            personCrying = condition3Names[10];
         } else {
            personCrying = condition3Names[11];
         } 
      } else if (numberOfOnIndicators > numberOfOffIndicators){
         Debug.LogFormat("[The Teardrop #{0}] Since the number of unlit indicators is greater than the number of unlit indicators, the fourth column should be used.", ModuleId);
            Debug.LogFormat("[The Teardrop #{0}] The first indicator in alphabetical order is {1}.", ModuleId, firstIndicator);
            if (firstIndicator == "BOB") {
            personCrying = condition4Names[0];
         } else if (firstIndicator == "CAR") {
            personCrying = condition4Names[1];
         } else if (firstIndicator == "CLR") {
            personCrying = condition4Names[2];
         } else if (firstIndicator == "FRK") {
            personCrying = condition4Names[3];
         } else if (firstIndicator == "FRQ") {
            personCrying = condition4Names[4];
         } else if (firstIndicator == "IND") {
            personCrying = condition4Names[5];
         } else if (firstIndicator == "MSA") {
            personCrying = condition4Names[6];
         } else if (firstIndicator == "NSA") {
            personCrying = condition4Names[7];
         } else if (firstIndicator == "SIG") {
            personCrying = condition4Names[8];
         } else if (firstIndicator == "SND") {
            personCrying = condition4Names[9];
         } else if (firstIndicator == "TRN") {
            personCrying = condition4Names[10];
         } else {
            personCrying = condition4Names[11];
         }
      } else {
         Debug.LogFormat("[The Teardrop #{0}] Since none of the previous conditions apply, the last column should be used.", ModuleId);
         Debug.LogFormat("[The Teardrop #{0}] The first indicator in alphabetical order is {1}.", ModuleId, firstIndicator);
         if (firstIndicator == "BOB") {
            personCrying = otherwiseNames[0];
         } else if (firstIndicator == "CAR") {
            personCrying = otherwiseNames[1];
         } else if (firstIndicator == "CLR") {
            personCrying = otherwiseNames[2];
         } else if (firstIndicator == "FRK") {
            personCrying = otherwiseNames[3];
         } else if (firstIndicator == "FRQ") {
            personCrying = otherwiseNames[4];
         } else if (firstIndicator == "IND") {
            personCrying = otherwiseNames[5];
         } else if (firstIndicator == "MSA") {
            personCrying = otherwiseNames[6];
         } else if (firstIndicator == "NSA") {
            personCrying = otherwiseNames[7];
         } else if (firstIndicator == "SIG") {
            personCrying = otherwiseNames[8];
         } else if (firstIndicator == "SND") {
            personCrying = otherwiseNames[9];
         } else if (firstIndicator == "TRN") {
            personCrying = otherwiseNames[10];
         } else {
            personCrying = otherwiseNames[11];
         }
      }

        Debug.LogFormat("[The Teardrop #{0}] The person crying is {1}.", ModuleId, personCrying);


    }

   void FinalAnswer(){
      personCryingInNumbers = ConvertLettersToAlphabetPositions(personCrying);
      for(int b = 0; b < personCryingInNumbers.Length; b++){
         sumOfLetterValuesOfPersonCrying += personCryingInNumbers[b];
      }

      Debug.Log(sumOfLetterValuesOfPersonCrying);

      correctWordInNumbers = ConvertLettersToAlphabetPositions(correctWord);
      for(int c = 0; c < correctWordInNumbers.Length; c++){
         sumOfLetterValuesOfDecryptedWord += correctWordInNumbers[c];
      }
      Debug.Log(sumOfLetterValuesOfDecryptedWord);
      finalAnswer = (sumOfLetterValuesOfPersonCrying + sumOfLetterValuesOfDecryptedWord)%10;
      Debug.LogFormat("[The Teardrop #{0}] {1} is the correct last digit to press the button on.", ModuleId, finalAnswer);
   }

   void TeardropPressed() {

      Teardrop.AddInteractionPunch();
      if (ModuleSolved){
         return;
      }
      string time = Bomb.GetFormattedTime();
      Debug.LogFormat("[The Teardrop #{0}] {1} is the current time.", ModuleId, time);
      char lastDigitChar = time[time.Length - 1];
      int lastDigit = int.Parse(lastDigitChar.ToString());
         if (lastDigit == finalAnswer){
            Solve();
            Debug.LogFormat("[The Teardrop #{0}] You pressed the button when the timer ended in {1}! Well done!", ModuleId, finalAnswer);
         } else {
            Strike();
            Debug.LogFormat("[The Teardrop #{0}] The last digit of the timer was {1}. Expected last digit was {2}. Please try again.", ModuleId, lastDigit, finalAnswer);
         }
   }


   void Solve () {
      Audio.PlaySoundAtTransform("TEARDROP SOLVED SOUND", transform);
      GetComponent<KMBombModule>().HandlePass();
   }

   void Strike () {
      GetComponent<KMBombModule>().HandleStrike();
   }
}

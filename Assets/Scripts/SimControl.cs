/*******************************************************************************
Author:    Benjamin Ellinger
DP Email:  bellinge@digipen.edu
Date:      2/1/2022
Course:    DES 212

Description:
	This file is part of the framework for the 1D Combat Simulator assignment in
	DES 212 (System Design Methods). It can be freely used and modified by students
	for that assignment.
	
    This component controls the entire combat simulation. This component is added to
    a game object whose only purpose to contain this functionality, but using a
    ScriptedObject would potentially be a more advanced	way of doing this.
	
*******************************************************************************/

//Standard Unity component libraries
using System.Collections; //Not needed in this file, but here just in case.
using System.Collections.Generic; //Not needed in this file, but here just in case.
using System.IO; //Needed for writing telemetry data to a file.
using UnityEngine; //The library that lets your access all of the Unity functionality.
using UnityEngine.UI; //This is here so we don't have to type out longer names for UI components.

//Inherits from MonoBehavior like all normal Unity components do...
//Remember that the class name MUST be identical to the file name!
public class SimControl : MonoBehaviour
{
    //Does the simulation start in Auto mode?
    public static bool AutoMode = false;
    //Does the simulation start in Fast mode?
    public static bool FastMode = false;
    //This is the delta time the simulation uses,
    //which is artificially increased when in fast mode.
    public static float DT;

    //How many different AI types (and therefore how many "fights") do we want?
    public int Fights = 6;
    private int FightCount = 0;
    public static bool SimOver = false; //Have all the fights been completed?
    public static string CurrentAI = "Random"; //What's the current type of AI for this fight?

    //How many rounds is each "fight"?
    public int Rounds = 6;
    private int RoundCount = 0;
    public static bool RoundOver = false; //Did the current round just end?
    public static bool RoundStart = false; //Is a new round just starting (make sure the player has time to find a target)?
    //How long a delay between rounds?
    public float RoundDelay = 3.0f;
    private float RoundTimer = 3.0f;

    //How far from the center of the screen is the "edge" of the arena?
    public static float EdgeDistance = 8.0f;
    //How far from the center of the screen do combatants start?
    public static float StartingX = 5.0f;
 
    //Telemetry data for an individual fight.
    public static int Victories = 0;
    public static int Defeats = 0;
    public static float DamageDone = 0;
    public static float TotalFightTime = 0;
    public static StreamWriter DataStream; //Stream used to write the data to a file.

    //Need a reference to the player, so we don't have to look it
    //up each time.
    public static Hero Player;

    //We will use the UI canvas a lot, so store a reference to it.
    public static GameObject Canvas;

	//References for text prefabs and enemy prefabs, so we don't
	//have to load them each time.
    public static GameObject InfoTextPrefab;
    public static GameObject StaticInfoTextPrefab;
    public static GameObject EnemyType1Prefab; //Should really be an array or dictionary.
    public static GameObject EnemyType2Prefab;
    public static GameObject EnemyType3Prefab;

    //Start is called before the first frame update
    void Start()
    {
        //Create a comma-separated value file to output telemetry data.
        //This can just then be directly opened in Excel.
        DataStream = new StreamWriter("FightData.csv", true);
        //Write some headers for our columns. You'll need more columns than this eventually.
        DataStream.WriteLine("AI TYPE,VICTORIES,DEFEATS,DPS,ROUND LENGTH");

        //Get a reference to the canvas (used for UI objects).
        Canvas = GameObject.Find("Canvas");

        //Get a reference to the player's game object.
        //Note that we use GetComponent so we don't have to do that
        //every time we want to access the Hero class functionality.
        Player = GameObject.Find("Hero").GetComponent<Hero>();

        //Load all the prefabs we are going to use.
        InfoTextPrefab = Resources.Load("Prefabs/InfoText") as GameObject;
        StaticInfoTextPrefab = Resources.Load("Prefabs/StaticInfoText") as GameObject;
        EnemyType1Prefab = Resources.Load("Prefabs/MeleeEnemy") as GameObject;
        EnemyType2Prefab = Resources.Load("Prefabs/SniperEnemy") as GameObject;
        EnemyType3Prefab = Resources.Load("Prefabs/EliteEnemy") as GameObject;
    }

    //Update is called once per frame
    void Update()
    {
        //If the ESC key is pressed, exit the program.
        if (Input.GetKeyDown(KeyCode.Escape) == true)
            Application.Quit();

        //The simulation is over, so stop updating.
        if (FightCount >= Fights)
        {
            if (SimOver == false) //Did the simulation just end?
            {
                SimOver = true;
                DataStream.Close(); //Don't forget to close the stream.
                SpawnInfoText("SIMULATION OVER", true);
            }
            return;
        }

        //If the A key is pressed, toggle Auto mode on or off.
        if (Input.GetKeyDown(KeyCode.A) == true)
            AutoMode = !AutoMode;

        //If the F key is pressed, toggle Fast Auto mode on or off.
        if (Input.GetKeyDown(KeyCode.F) == true)
            FastMode = !FastMode;

        //If the R key is pressed, restart the simulation.
        if (Input.GetKeyDown(KeyCode.R) == true)
        {
            FightCount = 0;
            RoundCount = 0;
            RoundTimer = RoundDelay;
            RoundStart = false;
            RoundOver = false;
            SimOver = false;
            CurrentAI = "Random";
            RoundCount = 0;
            Victories = 0;
            Defeats = 0;
            DamageDone = 0;
            TotalFightTime = 0;
        }

        //Get the actual delta time, but cap it at one-tenth of
        //a second. Except in fast mode, where we just make it
        //one-tenth of a second all the time. Note that if we make
        //this more than one-tenth of a second, we might get different
        //results in fast mode vs. normal mode by "jumping" over time
        //thresholds (cooldowns for example) that are in tenths of a second.
        if (FastMode)
            DT = 0.1f; //We could go even faster by not having visual feedback in this mode...
        else if (Time.deltaTime < 0.1f)
            DT = Time.deltaTime;
        else
            DT = 0.1f;

        //It's the start of a fight, so start a new round.
        if (RoundCount == 0)
            NewRound();

        RoundOver = IsRoundOver();
        if (RoundOver == false) //The round isn't over, so run the simulation (all the logic is in the updates of other classes).
            TotalFightTime += DT; //Accumulate the SIMULATED time for telemetry data.
        else if (RoundTimer > 0.0f) //The round is over, but this is the delay before a new round.
            RoundTimer -= DT; //Update the round delay timer.
        else //Time for a new round.
            NewRound();
    }

    //The round is over if either the player is dead or all enemies are.
    bool IsRoundOver()
    {
        //Player is dead.
        if (Player.HitPoints == 0.0f)
        {
            if (RoundOver == false) //Player just died.
            {
                SpawnInfoText("DEFEAT...");
                Defeats++;
            }
            return true;
        }
        //Enemies are dead.
        if (Player.Target == null)
        {
            if (RoundStart == true) //Make sure player has a chance to find a target at the start of a round.
                return false;
            if (RoundOver == false) //Last enemy just died.
            {
                SpawnInfoText("VICTORY!!!");
                Victories++;
            }
            return true;
        }
        //Round is not over.
        RoundStart = false;
        return false;
    }

	//Reset everything for the new round.
    void NewRound()
    {
        RoundCount++;
        //Clear out any remaining enemies.
        ClearEnemies();
        
        //The whole fight is over, so start a new one.
        if (RoundCount > Rounds)
        {
            NewFight();
            return;
        }

        //Spawn enemies by calling the Unity engine function Instantiate().
        //Pass in the appropriate prefab, its position, its rotation (90 degrees),
        //and its parent (none).
        //You'll really want these to be an array/dictionary of prefabs eventually.
        if (RoundCount % 6 == 1)
            Instantiate(EnemyType1Prefab, new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
        else if (RoundCount % 6 == 2)
            Instantiate(EnemyType2Prefab, new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
        else if (RoundCount % 6 == 3)
            Instantiate(EnemyType3Prefab, new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
        else if (RoundCount % 6 == 4)
        {
            //Adjust the starting X/Y a bit for groups.
            Instantiate(EnemyType1Prefab, new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyType1Prefab, new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyType1Prefab, new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);
        }
        else if (RoundCount % 6 == 5)
        {
            //Adjust the starting X/Y a bit for groups.
            Instantiate(EnemyType2Prefab, new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyType2Prefab, new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyType2Prefab, new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);
        }
        else if (RoundCount % 6 == 0)
        {
            //Adjust the starting X/Y a bit for groups.
            Instantiate(EnemyType3Prefab, new Vector3(StartingX + 1, -1.5f, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyType3Prefab, new Vector3(StartingX, 0, 0), Quaternion.Euler(0, 0, 90), null);
            Instantiate(EnemyType3Prefab, new Vector3(StartingX + 1, 1.5f, 0), Quaternion.Euler(0, 0, 90), null);
        }
        //Note that this just cycles through enemy types/groups, but you'll need more structure than this.
        //Each fight should be one AI type against one enemy type multiple times. And then each AI type
        //against a group of the same type multiple times. And then each AI type against a mixed group
        //multiple times. Group mode and mixed mode will require different logic, etc.

        //Call the Initialize() functions for the player.
        Player.Initialize();

        //Feedback is good...
        SpawnInfoText("ROUND " + RoundCount); //Look! A string concatenation operator!

        //Reset the round delay timer (and round start flag) for after this new round ends.
        RoundTimer = RoundDelay;
        RoundStart = true;
    }

    //Reset everything for the new fight.
    void NewFight()
    {
        FightCount++;
        RoundCount = 0;
        //Show a bit of telemetry data on screen.
        SpawnInfoText(Victories + "-" + Defeats + "\n" + DamageDone / TotalFightTime + " DPS");
        //Write all the telemetry data to the file.
        DataStream.WriteLine(CurrentAI + "," + Victories + "," + Defeats + "," + DamageDone / TotalFightTime + "," + TotalFightTime / Rounds);
        //Reset the telemetry counters
        Victories = 0;
        Defeats = 0;
        DamageDone = 0;
        TotalFightTime = 0;
        //After the first fight (which is random), just spam a single key for each fight.
        CurrentAI = "Spam" + FightCount;
    }

    //Destroy all the enemy game objects.
    void ClearEnemies()
    {
        //Find all the game objects that have an Enemy component.
        var enemies = FindObjectsOfType<Enemy>();
        if (enemies.Length == 0) //Didn't find any.
            return;
        foreach (Enemy enemy in enemies) //A foreach loop! Fancy...
            Destroy(enemy.gameObject);
    }

    //Spawn text at the center of the screen.
    //If set to static, that just means it doesn't move.
    void SpawnInfoText(string text, bool isStatic = false)
    {
        SpawnInfoText(new Vector3(0, 0, 0), text, isStatic);
    }

    //Spawn text wherever you want.
    //If set to static, that just means it doesn't move.
    void SpawnInfoText(Vector3 location, string text, bool isStatic = false)
    {
        //Throw up some text by calling the Unity engine function Instantiate().
        //Pass in the appropriate InfoText prefab, its position, its rotation (none in this case),
        //and its parent (the canvas because this is text). Then we get the
        //Text component from the new game object in order to set the text itself.
        Text infotext;
        if (isStatic)
            infotext = Instantiate(StaticInfoTextPrefab, location, Quaternion.identity, Canvas.transform).GetComponent<Text>();
        else
            infotext = Instantiate(InfoTextPrefab, location, Quaternion.identity, Canvas.transform).GetComponent<Text>();
        //Set the text.
        infotext.text = text;
    }
}
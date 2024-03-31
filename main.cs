using System;
using System.Threading;
using System.Linq;

namespace SealTown
{
    public static class Game
    {
        public static GameItems items = new GameItems(); // Get our game items

        public static Player player = new Player(); // Create the player.
        
        public static bool DebugMode = false;

        public static bool debugAuth = false;
    }
    public static class GlobalFunctions
    {
        public static T[] RemoveItemAt<T>(T[] originalArray, int index)
        {
            if (originalArray == null)
                throw new ArgumentNullException(nameof(originalArray));
            if (index < 0 || index >= originalArray.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            T[] newArray = new T[originalArray.Length - 1];
            int newArrayIndex = 0;
            for (int i = 0; i < originalArray.Length; i++)
            {
                if (i != index)
                {
                    newArray[newArrayIndex] = originalArray[i];
                    newArrayIndex++;
                }
            }
            return newArray;
        }
        public static void endGame()
        {
            throw new Exception("The game was ended.");
        }
        public static void parseCommand(string command)
        {
            string[] parsed = command.Split("-");
            switch (parsed[0])
            {
                case "grant":
                    if (parsed[1] == "player")
                    {
                        foreach (Item item in Game.items.allItems)
                        {
                            Console.WriteLine(item.name + " == " + parsed[2] + " - " + (item.name == parsed[2] ? "Y" : "N"));
                            if (item.name == parsed[2])
                            {
                                for (int i=0;i<int.Parse(parsed[3]);i++)
                                {
                                    Game.player.addItem(item);
                                }
                                if (item.recipeUnlocks != null)
                                {
                                    CraftingRecipe[] totalUnlocks = {};
                                    foreach (CraftingRecipe recipe in item.recipeUnlocks)
                                    {
                                        if (Array.IndexOf(Game.player.unlockedRecipes, recipe) < 0)
                                        {
                                            totalUnlocks = totalUnlocks.Concat(new CraftingRecipe[] {recipe}).ToArray();
                                        }
                                    }
                                    if (totalUnlocks != null)
                                    {
                                        Console.WriteLine("Unlocked Recipes:");
                                        foreach (CraftingRecipe recipe in totalUnlocks)
                                        {
                                            Console.WriteLine(" - " + recipe.name);
                                        }
                                        Game.player.unlockedRecipes = Game.player.unlockedRecipes.Concat(item.recipeUnlocks).ToArray();
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }
    }
    // ITEMS
    public class Item
    {
        public string name;
        public int itemSize;
        public int itemValue; // determines XP gain
        public CraftingRecipe[] recipeUnlocks;

        public void create(string newName, int newItemSize, CraftingRecipe[] newRecipeUnlocks=null, int newItemValue=1)
        {
            this.name = newName;
            this.itemSize = newItemSize;
            this.recipeUnlocks = newRecipeUnlocks;
            this.itemValue = newItemValue;
        }
    }
    public class CraftingRecipe
    {
        public string name;
        public Item[] itemsIn;
        public Item[] itemsOut;
        public CraftingRecipe[] recipeUnlocks;

        public void create(string newName, Item[] newItemsIn, Item[] newItemsOut, CraftingRecipe[] newRecipeUnlocks)
        {
            this.name = newName;
            this.itemsIn = newItemsIn;
            this.itemsOut = newItemsOut;
            this.recipeUnlocks = newRecipeUnlocks;
        }

        public Item[] craftItems(Item[] providedItems)
        {
            int itemsInProvided = 0;
            foreach (Item item in providedItems)
            {
                if (Array.IndexOf(this.itemsIn, item) > -1)
                {
                    itemsInProvided += 1;
                }
            }
            if (itemsInProvided >= this.itemsIn.Length)
            {
                return this.itemsOut;
            }
            else
            {
                return null;
            }
        }
        public bool craftable(Item[] providedItems)
        {
            int itemsInProvided = 0;
            foreach (Item item in providedItems)
            {
                if (Array.IndexOf(this.itemsIn, item) > -1)
                {
                    itemsInProvided += 1;
                }
            }
            if (itemsInProvided > this.itemsIn.Length - 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public class Generator
    {
        public string name;
        public Item[] generatorItems;
        public Item[] startItem;
        public Item[] requiredToUse;
        public bool started = false;

        public void create(string newName, Item[] newGeneratorItems, Item[] newStartItem, Item[] newRequiredToUse)
        {
            this.name = newName;
            this.generatorItems = newGeneratorItems;
            this.startItem =  newStartItem;
            this.requiredToUse = newRequiredToUse;
        }
        public Item[] takeItem(Item[] providedItems)
        {
            if (this.started)
            {
                if (this.requiredToUse == null)
                {
                    return this.generatorItems;
                }
                else
                {
                    int itemsFound = 0;
                    foreach (Item item in providedItems)
                    {
                        if (Array.IndexOf(this.requiredToUse, item) > -1)
                        {
                            itemsFound += 1;
                        }
                    }
                    if (itemsFound >= this.requiredToUse.Length)
                    {
                        return this.generatorItems;
                    }
                }
            }
            else
            {
                Console.WriteLine("Generator not started. Items Needed: ");
                if (this.startItem == null)
                {
                    Console.WriteLine("No items needed to start.");
                }
                else
                {
                    foreach (Item item in this.startItem)
                    {
                        Console.WriteLine(item.name);
                    }
                }
            }
            return null;
        }
        public void startGenerator(Item[] providedItems)
        {
            if (this.startItem == null)
            {
                this.started = true;
                Console.WriteLine("Generator Started.");
            }
            else
            {
                if (providedItems != null)
                {
                    int itemsFound = 0;
                    foreach (Item item in providedItems)
                    {
                        if (Array.IndexOf(this.startItem, item) > -1)
                        {
                            itemsFound += 1;
                        }
                    }
                    if (itemsFound >= this.startItem.Length)
                    {
                        this.started = true;
                        foreach (Item item in this.startItem)
                        {
                            Game.player.inventory = GlobalFunctions.RemoveItemAt(Game.player.inventory, Array.IndexOf(Game.player.inventory, item));
                        }
                        Console.WriteLine("Generator Started.");
                    }
                }
                else
                {
                    Console.WriteLine("Items not supplied.");
                }
            }
        }
    }

    public class Area
    {
        public string name;
        public Item[] itemsInArea;
        public Area[] accessibleAreas;
        public Generator[] generatorsInArea;
        public Item[] itemsRequiredToEnter;
        public Quest[] questsInArea;

        public void create(string newAreaName, Item[] newItemsInArea, Area[] newAccessibleAreas, Generator[] newGeneratorsInArea, Item[] newItemsRequiredToEnter, Quest[] newQuestsInArea=null)
        {
            this.name = newAreaName;
            this.itemsInArea = newItemsInArea;
            this.accessibleAreas = newAccessibleAreas;
            this.generatorsInArea = newGeneratorsInArea;
            this.itemsRequiredToEnter = newItemsRequiredToEnter;
            if (newQuestsInArea != null)
            {
                this.questsInArea = newQuestsInArea;
            }
        }
        public void viewItems()
        {
            try
            {
                Console.WriteLine("Items in area:");
                foreach (Item item in this.itemsInArea)
                {
                    Console.WriteLine(item.name);
                }
            }
            catch
            {
                Console.WriteLine("No items in area.");
            }
        }
        public Area moveAreas()
        {
            try
            {
                Console.WriteLine("Available Areas:");
                int foreachLoopID = 0;
                foreach (Area area in this.accessibleAreas)
                {
                    Console.WriteLine(area.name + " " + foreachLoopID.ToString());
                    foreachLoopID += 1;
                }
                Console.Write("#> ");
                string areaToMove = Console.ReadLine();
    
                Area areaMove = this.accessibleAreas[int.Parse(areaToMove)];
                int found = 0;
                try {
                    foreach (Item item in areaMove.itemsRequiredToEnter)
                    {
                        if (Array.IndexOf(Game.player.inventory, item) > -1)
                        {
                            found += 1;
                        }
                    }
                }
                catch
                {
                    return areaMove;
                }
                if (found >= areaMove.itemsRequiredToEnter.Length)
                {
                    return areaMove;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                Console.WriteLine("Invalid Input");
                return null;
            }
        }
        public Item[] takeItem()
        {
            Console.WriteLine("Take Item:");
            Console.WriteLine("Seperate Numbers With A Comma to Pickup Multiple Items");
            if (this.itemsInArea.Length > 0)
            {
                for (int i=0; i<(this.itemsInArea.Length); i++)
                {
                    Item item = this.itemsInArea[i];
                    Console.WriteLine(item.name + " " + i.ToString());
                }
                Console.Write("#> ");
                string itemToTake = Console.ReadLine();
                Item[] allItemReturns = {};
                string[] totalSplit = itemToTake.Split(new string[] {","}, 100000, StringSplitOptions.None);
                foreach (string currentIntToTake in totalSplit)
                {
                    int itemIDtoTake = int.Parse(currentIntToTake);

                    Item itemToReturn = this.itemsInArea[itemIDtoTake];

                    allItemReturns = allItemReturns.Concat(new Item[] {itemToReturn}).ToArray();
                }
                foreach (Item item in allItemReturns)
                {
                    this.itemsInArea = GlobalFunctions.RemoveItemAt(this.itemsInArea, Array.IndexOf(this.itemsInArea, item));
                }

                return allItemReturns;
            }
            else
            {
                Console.WriteLine("No items in area.");
                return null;
            }
        }
    }
    
    public class Quest
    {
        public string name;
        public Item[] itemsToComplete;
        public string rewardType; // unlock recipe, item, or generator start
        public Item[] itemReward;
        public CraftingRecipe[] recipeReward;
        public Generator[] generatorReward;
        public string xpRewardType;
        public int xpReward;

        public string[] startDialogue;
        public string[] noItemsDialogue;
        public string[] finishedDialogue;

        public bool started = false;
        public bool finished = false;

        public void create(string newName, Item[] newItemsToComplete, string newRewardType, Item[] newItemReward, CraftingRecipe[] newRecipeReward, Generator[] newGeneratorReward, string[] newDialogue1, string[] newDialogue2, string[] newDialogue3)
        {
            this.name = newName;
            this.itemsToComplete = newItemsToComplete;
            this.rewardType = newRewardType;
            this.itemReward = newItemReward;
            this.recipeReward = newRecipeReward;
            this.generatorReward = newGeneratorReward;
            this.startDialogue = newDialogue1;
            this.noItemsDialogue = newDialogue1;
            this.finishedDialogue = newDialogue3;
        }

        public void startQuest()
        {
            foreach (string dialoguePiece in this.startDialogue)
            {
                Console.Write(dialoguePiece + "(ENTR to continue)");
                Console.ReadLine();
            }
            this.started = true;
        }
        public void finishQuest()
        {
            Console.WriteLine("Quest " + this.name + " Completed. Reward:");
            switch (this.rewardType)
            {
                case "item":
                    Console.WriteLine("Items:");
                    foreach (Item item in this.itemReward)
                    {
                        Console.WriteLine(" - " + item.name);
                        Game.player.addItem(item);
                    }
                    break;
                case "generator":
                    Console.WriteLine("Generator Starts:");
                    foreach (Generator gen in this.generatorReward)
                    {
                        Console.WriteLine(" - " + gen.name);
                        gen.started = true;
                    }
                    break;
                case "recipe":
                    Console.WriteLine("Recipe Unlocks:");
                    foreach (CraftingRecipe recipe in this.recipeReward)
                    {
                        Console.WriteLine(" - " + recipe.name);
                        Game.player.unlockedRecipes = Game.player.unlockedRecipes.Concat(new CraftingRecipe[] { recipe }).ToArray();
                    }
                    break;
                case "xp":
                    Console.WriteLine("Currently deprecated");
                    break;
            }
        }

        public Item[] supplyItems(Item[] items)
        {
            Item[] returnList = {};
            int itemsSupplied = 0;
            foreach (Item item in items)
            {
                if (Array.IndexOf(this.itemsToComplete, item) > -1)
                {
                    itemsSupplied += 1;
                    this.itemsToComplete = GlobalFunctions.RemoveItemAt(this.itemsToComplete, Array.IndexOf(this.itemsToComplete, item));
                    returnList = GlobalFunctions.RemoveItemAt(items, Array.IndexOf(items, item));
                }
            }
            if (itemsSupplied == 0)
            {
                foreach (string str in this.noItemsDialogue)
                {
                    Console.WriteLine(str);
                }
            }
            if (itemsSupplied >= this.itemsToComplete.Length)
            {
                this.finishQuest();
            }
            return returnList;
        }
    }
 
    public class Player
    {
        public Area areaPosition = Game.items.Base;
        public Item[] inventory = {Game.items.wood};
        public int inventorySpace = 100;
        public int health = 100;

        // crafting xp, mining xp, combat xp, bartering xp
        public int[] experience = {
            0, // crafting
            0, // mining
            0, // combat
            0 // bartering
        };

        public CraftingRecipe[] unlockedRecipes = {Game.items.sticksRecipe, Game.items.fireRecipe};

        public void displayInventory()
        {
            Console.WriteLine("Current Inventory: ");
            Item[] totalItems = {};
            foreach (Item item in this.inventory)
            {
                if (Array.IndexOf(totalItems, item) < 0)
                {
                    totalItems = totalItems.Concat(new Item[] {item}).ToArray();
                }
            }
            int[] itemCounts = {};
            for (int i=0;i<totalItems.Length;i++)
            {
                int countForItem = 0;
                foreach (Item item2 in this.inventory)
                {
                    if (totalItems[i] == item2)
                    {
                        countForItem += 1;
                    }
                }
                itemCounts = itemCounts.Concat(new int[] {countForItem}).ToArray();
            }
            for (int i=0;i<totalItems.Length;i++)
            {
                Console.WriteLine(" - " + totalItems[i].name + " : " + itemCounts[i].ToString());
            }
        }
        public void addItem(Item item)
        {
            this.inventory = this.inventory.Concat(new Item[] {item}).ToArray();
        }
    }

    // GAME
    public class GameSession
    {
        public bool GameRunning = true;
        public void endGame()
        {
            throw new TimeoutException();
        }
        public void loadSave(string saveCode)
        {
            String[] seperator = {":"};
            Int32 count = 100000000;
            String[] saveSplit = saveCode.Split(seperator, count, StringSplitOptions.None);
            foreach (Area area in Game.items.allAreas)
            {
                if (area.name == saveSplit[0])
                {
                    Game.player.areaPosition = area;
                }
            }
            String[] seperator2 = {","};
            String[] saveSplit2 = saveSplit[1].Split(seperator2, count, StringSplitOptions.None);
            foreach (string itemNum in saveSplit2)
            {
                try
                {
                    int indexNum = int.Parse(itemNum);
                    Game.player.addItem(Game.items.allItems[indexNum]);
                }
                catch
                {
                    continue;
                }
            }
            String[] seperator3 = {","};
            String[] saveSplit3 = saveSplit[2].Split(seperator3, count, StringSplitOptions.None);
            foreach (String string4 in saveSplit3)
            {
                try
                {
                    int indexNum2 = int.Parse(string4);
                    CraftingRecipe recipeToUnlock = Game.items.allRecipes[indexNum2];
                    if (Array.IndexOf(Game.player.unlockedRecipes, recipeToUnlock) < 0)
                    {
                        Game.player.unlockedRecipes = Game.player.unlockedRecipes.Concat(new CraftingRecipe[] {recipeToUnlock}).ToArray();
                    }
                }
                catch
                {
                    continue;
                }
            }
            string[] seperator4 = {"="};
            string[] saveSplit4 = saveSplit[3].Split(seperator4, count, StringSplitOptions.None);
            foreach (string areaName in saveSplit4)
            {
                if (areaName != "")
                {
                    Area currentArea = null;
                    // Get name and items
                    string[] seperator5 = {"+"};
                    string[] saveSplit5 = areaName.Split(seperator5, count, StringSplitOptions.None);    
                    // Index 0 of SaveSplit 5 is area name
                    foreach (Area area in Game.items.allAreas)
                    {
                        if (area.name == saveSplit5[0])
                        {
                            currentArea = area;
                        }
                    }
                    // Individual item indexes
                    string[] seperator6 = {","};
                    string[] saveSplit6 = saveSplit5[1].Split(seperator6, count, StringSplitOptions.None);    
                    for (int i=0;i<saveSplit6.Length;i++)
                    {
                        try
                        {
                            Console.Write(" " + saveSplit6[i]);
                            Item itemToAddToArea = Game.items.allItems[int.Parse(saveSplit6[i])];
                            currentArea.itemsInArea = currentArea.itemsInArea.Concat(new Item[] {itemToAddToArea}).ToArray();
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    if (saveSplit6.Length == 1)
                    {
                        currentArea.itemsInArea = null;
                    }
                    Console.Write("\n");
                }
            }
            return;
        }
        public string saveGame()
        {
            Player player =  Game.player;
            string save = "";
            save = save + player.areaPosition.name;
            save = save + ":";
            foreach (Item item in player.inventory)
            {
                save = save + "," + Array.IndexOf(Game.items.allItems, item);
            }
            save = save + ":";
            foreach (CraftingRecipe recipe in player.unlockedRecipes)
            {
                save = save + "," + Array.IndexOf(Game.items.allRecipes, recipe);
            }

            save = save + ":";
            // Area items
            save = save + "=";
            foreach (Area area in Game.items.allAreas)
            {
                save = save + area.name;
                // items on ground
                save = save + "+";
                foreach (Item item in area.itemsInArea)
                {
                    save = save + Array.IndexOf(Game.items.allItems, item) + ",";
                }
                save = save + "=";
            }
            save = save + ":";
            return save;
        }
        public string gameIteration()
        {
            Console.WriteLine("Craft | Q\nMove | W\nInventory | E\nView Items | R\nPickup Items | T\nInspect Areas | A\nUse Generator | S\nStart Generator | D\nView Unlocked Recipes | Z\nView Available Quests | O\nView Current Quests | P\nSupply Quest | I\nSave Game | =\nEnd Game | \\");
            Console.Write("> ");
            return Console.ReadLine();
        }
        public void AutoSave()
        {
            Thread.Sleep(270000);
            Console.WriteLine("\nCopy the save code and RE-RUN THE GAME.");
            Console.WriteLine(this.saveGame());
            Console.WriteLine("STOP PLAYING, COPY SAVE, AND REPLAY GAME");
        }
        public void Run()
        {
            Thread thread = new Thread(this.AutoSave);
            thread.Start();
            Console.WriteLine(",---.          |    --.--               \n`---.,---.,---.|      |  ,---.. . .,---.\n    ||---',---||      |  |   || | ||   |\n`---'`---'`---^`---'  `  `---'`-'-'`   '"); // random ASCII art
            Console.WriteLine(" -----> A SealTech game by Matthew Carmichael < -----");
            Thread.Sleep(1000);
            Console.Write("Initializing Game");
            for (int i=0; i<7; i++)
            {
                Thread.Sleep(500);
                Console.Write(".");
            }
            Console.Write("\n");
            Thread.Sleep(400);
            Game.items.initializeItems();
            Console.WriteLine("Load from save? (Y/N)");
            Console.Write("> ");
            if (Console.ReadLine().ToUpper() == "Y")
            {
                Console.WriteLine("Input Save Code");
                Console.Write("> ");
                string input = Console.ReadLine();
                try
                {
                    this.loadSave(input);
                }
                catch
                {
                    Console.WriteLine("Most sincere apologies, but your save code is either invalid or corrupted. Make sure it's exactly as you copied it, with no imperfections.");
                    Console.WriteLine("NOTICE: ALL SAVE CODES FROM VERSIONS LESS THAN 1.0.2 ARE DEPRECIATED AND NO LONGER WORK.");
                }
            }
            Console.WriteLine("Loading Complete!");
            Console.WriteLine("Visit the website for a tutorial on how to play!\nMake sure you are playing on the latest release! Check the website for it!");
            Thread.Sleep(400);
            while (GameRunning)
            {
                Console.WriteLine("----------");
                string iterReturn = gameIteration();
                Console.WriteLine("----------");
                try
                {
                    switch (iterReturn.ToUpper())
                    {
                        case "E":
                            Game.player.displayInventory();
                            break;
                        case "R":
                            Game.player.areaPosition.viewItems();
                            break;
                        case "W":
                            Area newArea = Game.player.areaPosition.moveAreas();
                            if (newArea == null)
                            {
                                Console.WriteLine("You do not have the required items to enter.");
                            }
                            else
                            {
                                Game.player.areaPosition = newArea;
                            }
                            break;
                        case "T":
                            Item[] newItem = Game.player.areaPosition.takeItem();
                            try
                            {
                                Game.player.inventory = Game.player.inventory.Concat(newItem).ToArray();
                                foreach (Item item in newItem)
                                {
                                    if (item.recipeUnlocks != null)
                                    {
                                        CraftingRecipe[] totalUnlocks = {};
                                        foreach (CraftingRecipe recipe in item.recipeUnlocks)
                                        {
                                            if (Array.IndexOf(Game.player.unlockedRecipes, recipe) < 0)
                                            {
                                                totalUnlocks = totalUnlocks.Concat(new CraftingRecipe[] {recipe}).ToArray();
                                            }
                                        }
                                        if (totalUnlocks != null)
                                        {
                                            Console.WriteLine("Unlocked Recipes:");
                                            foreach (CraftingRecipe recipe in totalUnlocks)
                                            {
                                                Console.WriteLine(" - " + recipe.name);
                                            }
                                            Game.player.unlockedRecipes = Game.player.unlockedRecipes.Concat(item.recipeUnlocks).ToArray();
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                break;
                            }
                            break;
                        case "Q":
                            CraftingRecipe[] availableRecipes = Game.player.unlockedRecipes;
                            if (availableRecipes.Length > 0)
                            {
                                Console.WriteLine("Available Recipes: ");
                                for (int i=0; i<availableRecipes.Length; i++)
                                {
                                    Console.WriteLine(availableRecipes[i].name + " " + i.ToString());
                                    Console.WriteLine(" - Items In");
                                    foreach (Item item in availableRecipes[i].itemsIn)
                                    {
                                        Console.WriteLine("   - " + item.name + (Array.IndexOf(Game.player.inventory, item) > -1 ? " (Aquired)" : ""));
                                    }
                                    Console.WriteLine(" - Items Out");
                                    foreach (Item itemOut in availableRecipes[i].itemsOut)
                                    {
                                        Console.WriteLine("   - " + itemOut.name);
                                    }
                                }
                                Console.Write("#> ");
                                int craftID = int.Parse(Console.ReadLine());

                                Item[] newInventoryItems = Game.player.inventory;
                                foreach (Item item in availableRecipes[craftID].itemsIn)
                                {
                                    newInventoryItems = GlobalFunctions.RemoveItemAt(newInventoryItems, Array.IndexOf(newInventoryItems, item));
                                }
                                Game.player.inventory = newInventoryItems;
                                Game.player.inventory = Game.player.inventory.Concat(availableRecipes[craftID].itemsOut).ToArray();

                                Console.WriteLine("Crafted " + availableRecipes[craftID].name);
                                CraftingRecipe[] unlockedRecipeList = {};
                                try
                                {
                                    foreach (CraftingRecipe recipe in availableRecipes[craftID].recipeUnlocks)
                                    {
                                        unlockedRecipeList = unlockedRecipeList.Concat(new CraftingRecipe[] {recipe}).ToArray();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    break;
                                }
                                foreach (CraftingRecipe recipe in unlockedRecipeList)
                                {
                                    if (Array.IndexOf(Game.player.unlockedRecipes, recipe) < 0)
                                    {
                                        Console.WriteLine(" - Unlocked Recipe: " + recipe.name);
                                        Game.player.unlockedRecipes = Game.player.unlockedRecipes.Concat(new CraftingRecipe[] {recipe}).ToArray();
                                    }
                                }
                                break;
                            }
                            else
                            {
                                Console.WriteLine("No recipes available.");
                                break;
                            }
                        case "A":
                            Console.WriteLine("Areas Accessible: ");
                            foreach (Area area in Game.player.areaPosition.accessibleAreas)
                            {
                                Console.WriteLine(area.name);
                                Console.WriteLine(" - Items Required to Enter");
                                foreach (Item item in area.itemsRequiredToEnter)
                                {
                                    Console.WriteLine("   - " + item.name);
                                }
                                Console.WriteLine(" - Items in Area");
                                try
                                {
                                    foreach (Item item in area.itemsInArea)
                                    {
                                        Console.WriteLine("   - " + item.name);
                                    }
                                }
                                catch
                                {
                                    Console.WriteLine("   - No Items In Area");
                                }
                                Console.WriteLine(" - Generators in Area");
                                try
                                {
                                    foreach (Generator gen in area.generatorsInArea)
                                    {
                                        Console.WriteLine("   - " + gen.name);
                                    }
                                }
                                catch
                                {
                                    Console.WriteLine("   - No Generatorss In Area");
                                }
                            }
                            break;
                        case "S":
                            Console.WriteLine("Generators In Area: ");
                            for (int i=0;i<Game.player.areaPosition.generatorsInArea.Length; i++)
                            {
                                Generator gen = Game.player.areaPosition.generatorsInArea[i];
                                Console.WriteLine(gen.name + " " + i.ToString());
                                Console.WriteLine("- Required To Use");
                                try
                                {
                                    foreach (Item item in gen.requiredToUse)
                                    {
                                        Console.WriteLine("   - " + item .name);
                                    }
                                }
                                catch
                                {
                                    Console.WriteLine("   - No Items Needed to Use");
                                }
                                Console.WriteLine(gen.started ? " - Generator Started" : " - Generator Not Started");
                            }
                            Console.Write("#> ");
                            int generatorChosen = int.Parse(Console.ReadLine());
                            Item[] itemsRecievedFromGenerator = Game.player.areaPosition.generatorsInArea[generatorChosen].takeItem(Game.player.inventory);
                            if (itemsRecievedFromGenerator != null)
                            {
                                Game.player.inventory = Game.player.inventory.Concat(itemsRecievedFromGenerator).ToArray();
                            }
                            break;
                        case "D":
                            Console.WriteLine("Generators In Area: ");
                            for (int i=0;i<Game.player.areaPosition.generatorsInArea.Length; i++)
                            {
                                Generator gen = Game.player.areaPosition.generatorsInArea[i];
                                Console.WriteLine(gen.name + " " + i.ToString());
                                Console.WriteLine(" - Items Needed To Start");
                                try
                                {
                                    foreach (Item item in gen.startItem)
                                    {
                                        Console.WriteLine("   - " + item .name);
                                    }
                                }
                                catch
                                {
                                    Console.WriteLine("   - No Items Needed to Start");
                                }
                                Console.WriteLine(gen.started ? " - Generator Started" : " - Generator Not Started");
                            }
                            Console.Write("#> ");
                            int generatorChosen2 = int.Parse(Console.ReadLine());
                            Game.player.areaPosition.generatorsInArea[generatorChosen2].startGenerator(Game.player.inventory);
                            break;
                        case "Z":
                            Console.WriteLine("Unlocked Recipes: ");
                            foreach (CraftingRecipe recipe in Game.player.unlockedRecipes)
                            {
                                Console.WriteLine(recipe.name);
                                Console.WriteLine(" - Items In");
                                foreach (Item item in recipe.itemsIn)
                                {
                                    Console.WriteLine("   - " + item.name + (Array.IndexOf(Game.player.inventory, item) > -1 ? " (Aquired)" : ""));
                                }
                                Console.WriteLine(" - Items Out");
                                foreach (Item item in recipe.itemsOut)
                                {
                                    Console.WriteLine("   - " + item.name);
                                }
                            }
                            break;
                        case "O":
                            if (Game.player.areaPosition.questsInArea == null)
                            {
                                Console.WriteLine("No available quests in this area at the moment. Check back later!");
                                break;
                            }
                            Area areaCurr = Game.player.areaPosition;
                            int index = 0;
                            foreach (Quest quest in areaCurr.questsInArea)
                            {
                                if (!quest.finished)
                                {
                                    Console.WriteLine(quest.name +  " " + index);
                                    index += 1;
                                }
                            }
                            break;
                        case "=":
                            Console.Write("Copy save code: ");
                            Console.Write(this.saveGame() + "\n");
                            break;
                        case "..": /* ----- DEBUG MODE ----- */
                            if (Game.debugAuth == false)
                            {
                                Console.WriteLine("Authentication is required. Please enter your username and passcode.");
                                Console.Write("Username > ");
                                string username = Console.ReadLine();
                                Console.Write("Passcode > ");
                                string password = Console.ReadLine();
                                if (username == "Sealmichael" && password == "Iloveseals1")
                                {
                                    Console.WriteLine("Valid Authentication.");
                                    Game.debugAuth = true;
                                }
                                else
                                {
                                    Console.WriteLine("Invalid authenticaton.");
                                    break;
                                }
                            }
                            bool debugMenu = true;
                            while (debugMenu)
                            {
                                Console.WriteLine("----------");
                                Console.WriteLine("Fetch Recipe | Q\nFetch Recipe Info | W\nView All Recipes | S\nAdd Item To Area | Z\nGo To Area | X\nEnter Debug Mode | \\\nExit Debug Mode | \\\\\nExit Debug Menu | /");
                                    Console.Write("!> ");
                                    string debugInput = Console.ReadLine();
                                switch (debugInput.ToUpper())
                                {
                                    case "Q":
                                        Console.WriteLine("Input the recipe name. Recipe will be added to inventory.");
                                        Console.Write("> ");
                                        string recipeName = Console.ReadLine();
        
                                        CraftingRecipe recipeToFetch = null;
                                        foreach (CraftingRecipe recipe in Game.items.allRecipes)
                                        {
                                            if (recipe.name.ToUpper() == recipeName.ToUpper())
                                            {
                                                recipeToFetch = recipe;
                                            }
                                        }
                                        if (recipeToFetch == null)
                                        {
                                            Console.WriteLine("Couldn't find that recipe.");
                                            break;
                                        }
                                        Game.player.inventory = Game.player.inventory.Concat(recipeToFetch.itemsOut).ToArray();
                                        Console.WriteLine("Added " + recipeToFetch.name + " to inventory.");
                                        break;
                                    case "W":
                                        Console.WriteLine("Input the recipe name. Recipe info will be displayed.");
                                        Console.Write("> ");
                                        string recipeName2 = Console.ReadLine();
        
                                        CraftingRecipe recipeToFetch2 = null;
                                        foreach (CraftingRecipe recipe in Game.items.allRecipes)
                                        {
                                            if (recipe.name.ToUpper() == recipeName2.ToUpper())
                                            {
                                                recipeToFetch2 = recipe;
                                            }
                                        }
                                        if (recipeToFetch2 == null)
                                        {
                                            Console.WriteLine("Couldn't find that recipe.");
                                            break;
                                        }
                                        Console.WriteLine(recipeToFetch2.name);
                                        Console.WriteLine(" - Items In");
                                        foreach (Item item in recipeToFetch2.itemsIn)
                                        {
                                            Console.WriteLine("   - " + item.name);
                                        }
                                        Console.WriteLine(" - Items Out");
                                        foreach (Item item in recipeToFetch2.itemsOut)
                                        {
                                            Console.WriteLine("   - " + item.name);
                                        }
                                        break;
                                    case "S":
                                        Console.WriteLine("All Recipes: (Current Amount: " + Game.items.allRecipes.Length.ToString() + ")");
                                        foreach (CraftingRecipe recipe in  Game.items.allRecipes)
                                        {
                                            Console.WriteLine(recipe.name);
                                            Console.WriteLine(" - Items In");
                                            foreach (Item item in recipe.itemsIn)
                                            {
                                                Console.WriteLine("   - " + item.name);
                                            }
                                            Console.WriteLine(" - Items Out");
                                            foreach (Item item in recipe.itemsOut)
                                            {
                                                Console.WriteLine("   - " + item.name);
                                            }
                                        }
                                        break;
                                    case "Z":
                                        Area currentArea = Game.player.areaPosition;
                                        
                                        Console.WriteLine("Enter recipe name to add.");
                                        Console.Write("> ");
                                        string recipeNameToAdd = Console.ReadLine();
                                        CraftingRecipe recipeAdd = null;
                                        foreach (CraftingRecipe recipe in Game.items.allRecipes)
                                        {
                                            if (recipe.name == recipeNameToAdd)
                                            {
                                                recipeAdd = recipe;
                                            }
                                        }
                                        currentArea.itemsInArea = currentArea.itemsInArea.Concat(recipeAdd.itemsOut).ToArray();
                                        break;
                                    case "/":
                                        debugMenu = false;
                                        break;
                                    case "\\":
                                        Game.DebugMode = true;
                                        Console.WriteLine("Debug mode enabled.");
                                        break;
                                    case "\\\\":
                                        Game.DebugMode = false;
                                        Console.WriteLine("Debug mode disabled.");
                                        break;
                                    case "X":
                                        Console.WriteLine("Input the Area Name");
                                        Console.Write("> ");
                                        string inputName = Console.ReadLine();
                                        Area areaToTeleport = null;
                                        foreach (Area area in Game.items.allAreas)
                                        {
                                            if (area.name == inputName)
                                            {
                                                areaToTeleport = area;
                                            }
                                        }
                                        if (areaToTeleport == null)
                                        {
                                            Console.WriteLine("Couldn't find that area.");
                                        }
                                        else
                                        {
                                            Game.player.areaPosition = areaToTeleport;
                                        }
                                        break;
                                }
                            }
                            break;
                        default:
                            Console.WriteLine("Invalid input.");
                            break;
                        // end of switch
                    }
                }
                catch (Exception e)
                {
                    // Console.WriteLine("Something went wrong.");
                    Console.WriteLine("----------");
                    Console.WriteLine("Invalid input.");
                    if (Game.DebugMode)
                    {
                        Console.WriteLine(e);
                        Console.WriteLine("You are seing this because debug mode is enabled");
                    }
                }
            }
        }
    }
    public class Init
    {
        static void Main()
        {
            GameSession game = new GameSession();
            game.Run();
        }
    }
    public class GameItems
    {
        // Initialize ALL game items.

        // Base Items
        public Item stone = new Item();
        public Item wood = new Item();
        public Item coal = new Item();
        public Item water = new Item();
        public Item ironOre = new Item();

        // Craftable Base Items
        public Item fire = new Item();
        public CraftingRecipe fireRecipe = new CraftingRecipe();

        // Tools
        public Item stoneAxe = new Item();
        public CraftingRecipe stoneAxeRecipe = new CraftingRecipe();
        public Item stoneHammer = new Item();
        public CraftingRecipe stoneHammerRecipe = new CraftingRecipe();
        public Item stoneChisel = new Item();
        public CraftingRecipe stoneChiselRecipe = new CraftingRecipe();
        
        // Stone Products
        public Item grindedStone = new Item();
        public CraftingRecipe grindedStoneRecipe = new CraftingRecipe();

        // Clay Products
        public Item clay = new Item();
        public CraftingRecipe clayRecipe = new CraftingRecipe();
        public Item clayCastingMold = new Item();
        public CraftingRecipe clayCastingMoldRecipe = new CraftingRecipe();

        public Item smeltingOven = new Item();
        public CraftingRecipe smeltingOvenRecipe = new CraftingRecipe();

        // Wood products
        public Item woodPlanks = new Item();
        public CraftingRecipe woodPlanksRecipe = new CraftingRecipe();
        public Item sticks = new Item();
        public CraftingRecipe sticksRecipe = new CraftingRecipe();

        // Iron Products
        public Item ironIngot = new Item();
        public CraftingRecipe ironIngotRecipe = new CraftingRecipe();
        public Item moltenIron = new Item();
        public CraftingRecipe moltenIronRecipe = new CraftingRecipe();

        public Item ironDagger = new Item();
        public CraftingRecipe ironDaggerRecipe = new CraftingRecipe();
        public Item ironSword = new Item();
        public CraftingRecipe ironSwordRecipe = new CraftingRecipe();

        // Tungsten Products
        public Item tungstenOre = new Item();
        public Item moltenTungsten = new Item();
        public CraftingRecipe moltenTungstenRecipe = new CraftingRecipe();
        public Item tungstenBar = new Item();
        public CraftingRecipe tungstenBarRecipe = new CraftingRecipe();
        public Item tungstenRod = new Item();
        public CraftingRecipe tungstenRodRecipe = new CraftingRecipe();

        public Item tungstenScrewdriver = new Item();
        public CraftingRecipe tungstenScrewdriverRecipe = new CraftingRecipe();

        public Item tungstenDrillHead = new Item();
        public CraftingRecipe tungstenDrillHeadRecipe = new CraftingRecipe();

        public Item tungstenDrill = new Item();
        public CraftingRecipe tungstenDrillRecipe = new CraftingRecipe();

        // Titanium Products
        public Item titaniumOre = new Item();
        public Item titaniumIngot = new Item();
        public CraftingRecipe titaniumIngotRecipe = new CraftingRecipe();
        public Item purifiedTitanium = new Item();
        public CraftingRecipe purifiedTitaniumRecipe = new CraftingRecipe();

        // Chemicals
        public Item red1 = new Item();
        public Item green2 = new Item();
        public Item blue3 = new Item();

        public Item sealinumCarbonate = new Item(); // Red1, Blue3
        public CraftingRecipe sealinumCarbonateRecipe = new CraftingRecipe();
        public Item sealinumCarbonateDioxide = new Item(); // SC, green2, blue3
        public CraftingRecipe sealinumCarbonateDioxideRecipe = new CraftingRecipe();
        public Item sealinumDitrate = new Item(); // SCD, SC, red1
        public CraftingRecipe sealinumDitrateRecipe = new CraftingRecipe();

        public Item fishidoul = new Item(); // Red1, Green2
        public CraftingRecipe fishidoulRecipe = new CraftingRecipe();
        public Item fishidoulNitrate = new Item(); // FD, Red1, SD
        public CraftingRecipe fishidoulNitrateRecipe = new CraftingRecipe();
        public Item fishidoulCarbonate = new Item(); // FN, FD, SCD
        public CraftingRecipe fishidoulCarbonateRecipe = new CraftingRecipe();

        public Item suspiciousMatter = new Item(); // FD, FN, SD, SCD
        public CraftingRecipe suspiciousMatterRecipe = new CraftingRecipe();
        public Item unstableMatter = new Item(); // SM, Red1, Green2, Blue3, FN, SD
        public CraftingRecipe unstableMatterRecipe = new CraftingRecipe();
        public Item antiMatter = new Item(); // SM, UM, SC
        public CraftingRecipe antiMatterRecipe = new CraftingRecipe();
        public Item uncontainableMatter = new Item(); // AM, UM, SD, FC
        public CraftingRecipe uncontainableMatterRecipe = new CraftingRecipe();

        // Void Products
        public Item voidShard = new Item(); // AM, UM, UCM, SC, Red1
        public CraftingRecipe voidShardRecipe = new CraftingRecipe();
        public Item voidConstruct = new Item(); // VS, UCM, SM
        public CraftingRecipe voidConstructRecipe = new CraftingRecipe();
        public Item unstableVoid = new Item(); // VC, VS, UCM, UM
        public CraftingRecipe unstableVoidRecipe = new CraftingRecipe();
        public Item voidStabilizer = new Item(); // Red1, Green2, SC, SCD, FD, FC
        public CraftingRecipe voidStabilizerRecipe = new CraftingRecipe();
        public Item containedVoidFragment = new Item(); // VD, UV, VST, UM, AM
        public CraftingRecipe containedVoidFragmentRecipe = new CraftingRecipe();
        public Item containedVoid = new Item(); // CVF, CVF, CVF, CVF, VST, AM, SC
        public CraftingRecipe containedVoidRecipe = new CraftingRecipe();
        public Item stableVoid = new Item(); // CV, VST, SD
        public CraftingRecipe stableVoidRecipe = new CraftingRecipe();
        public Item voidItem = new Item(); // SV, VC, VST, SC
        public CraftingRecipe voidItemRecipe = new CraftingRecipe();

        // Random Products
        public Item sandPaper = new Item();
        public CraftingRecipe sandPaperRecipe = new CraftingRecipe();

        public Item labKeyFragment = new Item();
        public Item labKey = new Item();
        public CraftingRecipe labKeyRecipe = new CraftingRecipe();

        // Crafting Recipes
        public CraftingRecipe[] allRecipes;
        // Items
        public Item[] allItems;
        // Areas
        public Area[] allAreas;

        // Areas
        public Area Base = new Area();
        public Area CC = new Area();
        public Area CC2 = new Area();
        public Area Passage1 = new Area();
        public Area RVillage = new Area();
        public Area SecretBunker = new Area();
        public Area SecretLab = new Area();
        public Area LabStorage = new Area();

        // Generators
        public Generator woodGenerator = new Generator();
        public Generator coalGenerator = new Generator();
        public Generator stoneGenerator = new Generator();
        public Generator waterGenerator = new Generator();
        public Generator fireGenerator = new Generator();
        public Generator tungstenGenerator = new Generator();
        public Generator titaniumGenerator = new Generator();

        public Generator redGenerator = new Generator();
        public Generator greenGenerator = new Generator();
        public Generator blueGenerator = new Generator();

        // Quests
        public Quest testQuest = new Quest();

        // This is called on game start
        public void initializeItems()
        {
            // Base Items
            this.stone.create("Stone", 1);
            this.wood.create("Wood", 1);
            this.coal.create("Coal", 1);
            this.water.create("Water", 1);
            this.ironOre.create("Iron Ore", 1, new CraftingRecipe[] {ironIngotRecipe});
            
            
            // Craftable Base Items
            this.fire.create("Fire", 1);
            this.fireRecipe.create("Fire", new Item[] {wood, coal}, new Item[] {fire}, null);
            
            
            // Tools
            this.stoneAxe.create("Stone Axe", 1);
            this.stoneAxeRecipe.create("Stone Axe", new Item[] {stone, sticks}, new Item[] {stoneAxe}, null);
            this.stoneHammer.create("Stone Hammer", 1);
            this.stoneHammerRecipe.create("Stone Hammer", new Item[] {stone, sticks}, new Item[] {stoneHammer}, new CraftingRecipe[] {grindedStoneRecipe, clayRecipe});
            this.stoneChisel.create("Stone Chisel", 1);
            this.stoneChiselRecipe.create("Stone Chisel", new Item[] {stone, sticks}, new Item[] {stoneChisel}, null);
            
            // Stone Products
            this.grindedStone.create("Grinded Stone", 1);
            this.grindedStoneRecipe.create("Grinded Stone", new Item[] {stone, stoneHammer}, new Item[] {grindedStone, stoneHammer}, null);

            // Clay Products
            this.clay.create("Clay", 1);
            this.clayRecipe.create("Clay", new Item[] {grindedStone, water}, new Item[] {clay}, new CraftingRecipe[] {clayCastingMoldRecipe});
            this.clayCastingMold.create("Clay Casting Mold", 1);
            this.clayCastingMoldRecipe.create("Clay Casting Mold", new Item[] {clay, fire, stoneChisel}, new Item[] {clayCastingMold, stoneChisel}, null);
            
            // Wood products
            this.woodPlanks.create("Wood Planks", 1);
            this.woodPlanksRecipe.create("Wood Planks", new Item[] {wood}, new Item[] {woodPlanks}, null);
            this.sticks.create("Sticks", 1);
            this.sticksRecipe.create("Sticks", new Item[] {wood}, new Item[] {sticks}, new CraftingRecipe[] {stoneHammerRecipe, stoneAxeRecipe, stoneChiselRecipe});
            
            // Iron Products
            this.ironIngot.create("Iron Ingot", 1);
            this.ironIngotRecipe.create("Iron Ingot", new Item[] {fire, ironOre}, new Item[] {ironIngot}, new CraftingRecipe[] {moltenIronRecipe});
            this.moltenIron.create("Molten Iron", 1);
            this.moltenIronRecipe.create("Molten Iron", new Item[] {fire, ironIngot}, new Item[] {moltenIron}, new CraftingRecipe[] {ironDaggerRecipe, ironSwordRecipe});
            
            this.ironDagger.create("Iron Dagger", 1);
            this.ironDaggerRecipe.create("Iron Dagger", new Item[] {clayCastingMold, moltenIron}, new Item[] {ironDagger}, null);
            this.ironSword.create("Iron Sword", 1);
            this.ironSwordRecipe.create("Iron Sword", new Item[] {clayCastingMold, moltenIron, ironDagger}, new Item[]  {ironSword}, null);
            
            // Tungsten Products
            tungstenOre.create("Tungsten Ore", 1);
            moltenTungsten.create("Molten Tungsten", 1);
            moltenTungstenRecipe.create("Molten Tungsten", new Item[] {tungstenOre, fire}, new Item[] {moltenTungsten}, new CraftingRecipe[] {tungstenBarRecipe, tungstenRodRecipe});
            tungstenBar.create("Tungsten Bar", 1);
            tungstenBarRecipe.create("Tungsten Bar", new Item[] {moltenTungsten, water, clayCastingMold}, new Item[] {tungstenBar}, null);
            tungstenRod.create("Tungsten Rod", 1);
            tungstenRodRecipe.create("Tungsten Rod", new Item[] {moltenTungsten, water, clayCastingMold}, new Item[] {tungstenRod}, null);

            tungstenScrewdriver.create("Tungsten Screwdriver", 1);
            tungstenScrewdriverRecipe.create("Tungsten Screwdriver", new Item[] {tungstenRod, tungstenBar, fire}, new Item[] {tungstenScrewdriver}, new CraftingRecipe[] {tungstenDrillHeadRecipe, tungstenDrillRecipe});

            tungstenDrillHead.create("tungsten Drillhead", 1);
            tungstenDrillHeadRecipe.create("Tungsten Drillhead", new Item[] {moltenTungsten, tungstenRod, tungstenScrewdriver, water, sandPaper}, new Item[] {tungstenDrillHead, tungstenScrewdriver}, null);

            tungstenDrill.create("Tungsten Drill", 1);
            tungstenDrillRecipe.create("Tungsten Drill", new Item[] {tungstenDrillHead, tungstenScrewdriver, tungstenRod, water}, new Item[] {tungstenDrill, tungstenScrewdriver}, null);

            // Titanium Products
            titaniumOre.create("Titanium Ore", 1);
            titaniumIngot.create("Titanium Ingot", 1);
            titaniumIngotRecipe.create("Titanium Ingot", new Item[] {titaniumOre, fire}, new Item[] {titaniumIngot}, new CraftingRecipe[] {purifiedTitaniumRecipe});
            purifiedTitanium.create("Purified Titanium", 1);
            purifiedTitaniumRecipe.create("Purified Titanium", new Item[] {titaniumIngot, water, fire}, new Item[] {purifiedTitanium}, null);

            // Chemicals
            red1.create("Red1", 0);
            green2.create("Green2", 0);
            blue3.create("Blue3", 0);

            sealinumCarbonate.create("Sealinum Carbonate", 0); // Red1, Blue3
            sealinumCarbonateRecipe.create("Sealinum Carbonate", new Item[] {red1, blue3}, new Item[] {sealinumCarbonate}, new CraftingRecipe[] {sealinumCarbonateDioxideRecipe, sealinumDitrateRecipe, fishidoulRecipe, fishidoulNitrateRecipe, fishidoulCarbonateRecipe, suspiciousMatterRecipe, unstableMatterRecipe, antiMatterRecipe, uncontainableMatterRecipe, voidShardRecipe, voidConstructRecipe, unstableVoidRecipe, voidStabilizerRecipe, containedVoidFragmentRecipe, containedVoidRecipe, stableVoidRecipe, voidItemRecipe});
            sealinumCarbonateDioxide.create("Sealinum Carbonate Dioxide", 0); // SC, green2, blue3
            sealinumCarbonateDioxideRecipe.create("Sealinum Carbonate Dioxide", new Item[] {sealinumCarbonate, green2, blue3}, new Item[] {sealinumCarbonateDioxide}, null);
            sealinumDitrate.create("Sealinum Ditrate", 0); // SCD, SC, red1
            sealinumDitrateRecipe.create("Sealinum Ditrate", new Item[] {sealinumCarbonateDioxide, sealinumCarbonate, red1}, new Item[] {sealinumDitrate}, null);

            fishidoul.create("Fishidoul", 0); // Red1, Green2
            fishidoulRecipe.create("Fishidoul", new Item[] {red1, green2}, new Item[] {fishidoul}, null);
            fishidoulNitrate.create("Fishidoul Nitrate", 0); // FD, Red1, SD
            fishidoulNitrateRecipe.create("Fishidoul Nitrate", new Item[] {fishidoul, red1, sealinumDitrate}, new Item[] {fishidoulNitrate}, null);
            fishidoulCarbonate.create("Fishidoul Carbonate", 0); // FN, FD, SCD
            fishidoulCarbonateRecipe.create("Fishidoul Carbonate", new Item[] {fishidoul, fishidoulNitrate, sealinumCarbonateDioxide}, new Item[] {fishidoulCarbonate}, null);

            suspiciousMatter.create("Suspicous Matter", 0); // FD, FN, SD, SCD
            suspiciousMatterRecipe.create("Suspicious Matter", new Item[] {fishidoul, fishidoulNitrate, sealinumDitrate, sealinumCarbonateDioxide}, new Item[] {suspiciousMatter}, null);
            unstableMatter.create("Unstable Matter", 0); // SM, Red1, Green2, Blue3, FN, SD
            unstableMatterRecipe.create("Unstable Matter", new Item[] {suspiciousMatter, red1, green2, blue3, fishidoulNitrate, sealinumDitrate}, new Item[] {unstableMatter}, null);
            antiMatter.create("Antimatter", 0); // SM, UM, SC
            antiMatterRecipe.create("Antimatter", new Item[] {suspiciousMatter, unstableMatter, sealinumCarbonate}, new Item[] {antiMatter}, null);
            uncontainableMatter.create("Uncontainable Matter", 0); // AM, UM, SD, FC
            uncontainableMatterRecipe.create("Uncontainable Matter", new Item[] {antiMatter, unstableMatter, sealinumDitrate, fishidoulCarbonate}, new Item[] {uncontainableMatter}, null);

            // Void Products
            voidShard.create("Void Shard", 0); // AM, UM, UCM, SC, Red1
            voidShardRecipe.create("Void Shard", new Item[] {antiMatter, unstableMatter, uncontainableMatter, sealinumCarbonate, red1}, new Item[] {voidShard}, null);
            voidConstruct.create("Void Construct", 0); // VS, UCM, SM
            voidConstructRecipe.create("Void Construct", new Item[] {voidShard, uncontainableMatter, suspiciousMatter}, new Item[] {voidConstruct}, null);
            unstableVoid.create("Unstable Void", 0); // VC, VS, UCM, UM
            unstableVoidRecipe.create("Unstable Void", new Item[] {voidConstruct, voidShard, uncontainableMatter, unstableMatter}, new Item[] {unstableVoid}, null);
            voidStabilizer.create("Void Stabilizer", 0); // Red1, Green2, SC, SCD, FD, FC
            voidStabilizerRecipe.create("Void Stabilizer", new Item[] {red1, green2, sealinumCarbonate, sealinumCarbonateDioxide}, new Item[] {voidStabilizer}, null);
            containedVoidFragment.create("Contained Void Fragment", 0); // VD, UV, VST, UM, AM
            containedVoidFragmentRecipe.create("Contained Void Fragment", new Item[] {voidConstruct, unstableVoid, voidStabilizer, unstableMatter, antiMatter}, new Item[] {containedVoidFragment}, null);
            containedVoid.create("Contained Void", 0); // CVF, CVF, CVF, CVF, VST, AM, SC
            containedVoidRecipe.create("Contained Void", new Item[] {containedVoidFragment, containedVoidFragment, containedVoidFragment, containedVoidFragment, antiMatter, sealinumCarbonate}, new Item[] {containedVoid}, null);
            stableVoid.create("Stable Void", 0); // CV, VST, SD
            stableVoidRecipe.create("Stable Void", new Item[] {containedVoid, voidStabilizer, sealinumDitrate}, new Item[] {stableVoid}, null);
            voidItem.create("Void", 0); // SV, VC, VST, SC
            voidItemRecipe.create("Void", new Item[] {stableVoid, voidConstruct, voidStabilizer, sealinumCarbonate}, new Item[] {voidItem}, null);

            // Random Products
            labKeyFragment.create("Lab Key Fragment", 0);
            labKey.create("Lab Key", 0);
            labKeyRecipe.create("Lab Key", new Item[] {labKeyFragment, labKeyFragment, labKeyFragment, labKeyFragment}, new Item[] {labKey}, null);


            sandPaper.create("Sand Paper", 1);
            sandPaperRecipe.create("Sand Paper", new Item[] {stone, grindedStone, fire}, new Item[] {sandPaper}, null);

            // Quests
            // string newName, Item[] newItemsToComplete, string newRewardType, Item[] newItemReward, CraftingRecipe[] newRecipeReward, Generator[] newGeneratorReward, string[] newDialogue1, string[] newDialogue2, string[] newDialogue3

            testQuest.create("Test Quest", new Item[] {wood}, "item", new Item[] {stone}, null, null, new string[] {"Sup"}, new string[] {"Sup2"}, new string[] {"Sup3"});

            this.allRecipes = new CraftingRecipe[] {
                this.fireRecipe, // Base materials
                this.ironIngotRecipe, this.moltenIronRecipe, this.ironDaggerRecipe, this.ironSwordRecipe, // Iron Products
                this.sticksRecipe, this.woodPlanksRecipe, // Wood Materials
                this.stoneHammerRecipe,this.stoneAxeRecipe, this.stoneChiselRecipe, // Stone Tools
                this.grindedStoneRecipe, this.clayRecipe, this.clayCastingMoldRecipe, // Clay Products
                this.moltenTungstenRecipe, this.tungstenBarRecipe, this.tungstenRodRecipe, // Raw Tungsten Products
                this.tungstenDrillHeadRecipe, this.tungstenDrillRecipe, // Tungsten Products
                this.tungstenScrewdriverRecipe, // Tungsten Tools
                this.titaniumIngotRecipe, this.purifiedTitaniumRecipe, // Raw Titanium Products
                this.sealinumCarbonateRecipe, sealinumCarbonateDioxideRecipe, sealinumCarbonateDioxideRecipe, sealinumDitrateRecipe, fishidoulRecipe, fishidoulNitrateRecipe, fishidoulCarbonateRecipe, // Chemical Products
                suspiciousMatterRecipe, unstableMatterRecipe, antiMatterRecipe, uncontainableMatterRecipe,  // Matter Products
                voidShardRecipe, voidConstructRecipe, unstableVoidRecipe, voidStabilizerRecipe, containedVoidFragmentRecipe, containedVoidRecipe, stableVoidRecipe, voidItemRecipe, // Void products
                this.sandPaperRecipe, this.labKeyRecipe // Random Products
            };

            this.allItems = new Item[] {
                this.stone, this.wood, this.coal, this.water, this.ironOre, // Base materials
                this.fire, // Craftable base items
                this.stoneAxe, this.stoneHammer, this.stoneChisel, // Stone tools
                this.grindedStone, // Stone products
                this.clay, this.clayCastingMold, // Clay Products
                this.woodPlanks, this.sticks, // Wood products
                this.ironIngot, this.moltenIron, this.ironDagger, this.ironSword, // Iron Products
                this.tungstenOre, this.moltenTungsten, this.tungstenBar, this.tungstenRod, // Raw Tungsten Products
                this.tungstenDrillHead, this.tungstenDrill, // Tungsten Products
                this.tungstenScrewdriver, // Tungsten Tools
                this.titaniumOre, this.titaniumIngot, this.purifiedTitanium, // Raw Titanium Products
                this.red1, this.green2, this.blue3, // Raw Chemical Products
                this.sealinumCarbonate, this.sealinumCarbonateDioxide, this.sealinumDitrate, // Sealinum Products
                this.fishidoul, this.fishidoulNitrate, this.fishidoulCarbonate, // Fishidoul Products
                this.sandPaper, this.labKeyFragment, this.labKey// Random Products
            };

            this.allAreas = new Area[] {
                this.Base, this.CC, this.CC2, this.RVillage, this.Passage1, this.SecretBunker, this.SecretLab, this.LabStorage
            };

            // (name) (items) (accessibleAreas) (generators) (required to enter)
            Base.create("Base", new Item[] {wood, coal, stone, stone, ironOre}, new Area[] {CC, RVillage}, new Generator[] {woodGenerator, coalGenerator}, null, new Quest[] {testQuest});
            CC.create("Crystal Caverns", new Item[] {water, water}, new Area[] {CC2, Base}, new Generator[] {stoneGenerator, coalGenerator, fireGenerator}, new Item[] {stoneHammer});
            CC2.create("Crystal Caverns 2", new Item[] {water, water}, new Area[] {CC, Passage1}, new Generator[] {stoneGenerator, coalGenerator, fireGenerator, tungstenGenerator}, new Item[] {stoneHammer});
            RVillage.create("Rainstorm Village", new Item[] {}, new Area[] {Base}, new Generator[] {waterGenerator, fireGenerator}, new Item[] {stoneChisel});
            Passage1.create("Untold Passage", new Item[] {water, titaniumOre, labKeyFragment}, new Area[] {SecretBunker, CC}, null, null);
            SecretBunker.create("Secret Bunker", new Item[] {titaniumOre, sandPaper, labKeyFragment}, new Area[] {Passage1}, new Generator[] {titaniumGenerator}, new Item[] {tungstenScrewdriver});
            SecretLab.create("Secret Lab", new Item[] {red1, green2, blue3, labKeyFragment}, new Area[] {SecretBunker, LabStorage}, null, new Item[] {purifiedTitanium});
            LabStorage.create("Lab Storage", new Item[] {labKeyFragment}, new Area[] {SecretLab}, new Generator[] {redGenerator, blueGenerator, greenGenerator}, null);


            // (name) (itemsOut) (startItem) (requiredToUse)
            fireGenerator.create("Fire Pit", new Item[] {fire}, null, null);
            waterGenerator.create("Pond", new Item[] {water}, null, null);
            woodGenerator.create("Wood Generator", new Item[] {wood}, null, null);
            coalGenerator.create("Coal Mine", new Item[] {coal}, new Item[] {wood}, null);
            stoneGenerator.create("Stone Mine", new Item[] {stone}, null, new Item[] {stoneHammer});
            tungstenGenerator.create("Tungsten Mine", new Item[] {tungstenOre}, null, new Item[] {stoneHammer});
            titaniumGenerator.create("Titanium Mine", new Item[] {titaniumOre}, null, new Item[] {tungstenDrill});

            redGenerator.create("Red1 Barrel", new Item[] {red1}, null, new Item[] {labKey});
            greenGenerator.create("Green1 Barrel", new Item[] {green2}, null, new Item[] {labKey});
            blueGenerator.create("Blue1 Barrel", new Item[] {blue3}, null, new Item[] {labKey});
        }
    }
}

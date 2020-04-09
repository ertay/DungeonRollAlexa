using System.Collections.Generic;

namespace DungeonRollAlexa.Helpers
{
    public static class SoundManager
    {
        public static string DragonSound(bool playSounds)
        {
            if (!playSounds)
                return "";


            List<string> sounds = new List<string>();
        sounds.Add("<audio src=\"soundbank://soundlibrary/monsters/evolution_monsters/evolution_monsters_02\"/>");
            sounds.Add("<audio src=\"soundbank://soundlibrary/monsters/evolution_monsters/evolution_monsters_05\"/>");
            sounds.Add("<audio src=\"soundbank://soundlibrary/monsters/ghosts_demons/ghosts_demons_02\"/>");
            sounds.Add("<audio src=\"soundbank://soundlibrary/monsters/ghosts_demons/ghosts_demons_03\"/>");
            sounds.Add("<audio src=\"soundbank://soundlibrary/monsters/ghosts_demons/ghosts_demons_06\"/>");
            sounds.Add("<audio src=\"soundbank://soundlibrary/monsters/giant_monster/giant_monster_01\"/>");
            sounds.Add("<audio src=\"soundbank://soundlibrary/monsters/giant_monster/giant_monster_02\"/>");
            sounds.Add("<audio src=\"soundbank://soundlibrary/animals/dinosaurs/dinosaurs_10\"/>");
            
            int random = ThreadSafeRandom.ThisThreadsRandom.Next(sounds.Count);
            return sounds[random];
        }

    public static string DragonDeathSound(bool playSound)
    {
        if (!playSound)
            return "";
        
        return "<audio src=\"soundbank://soundlibrary/monsters/giant_monster/giant_monster_13\"/>";
        }

    public static string DiceRollSound(bool playSound)
    {
        if (!playSound)
            return "";

        
        return "<audio src=\"soundbank://soundlibrary/toys_games/board_games/board_games_08\"/>";
    }


    }
}


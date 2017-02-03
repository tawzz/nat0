using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ations
{
  public partial class Game
  {
    // base scoring america = 7.4, 2w 5c 7g 5vp, egypt = 2w 7c 5g 4vp
    string startWithTest = "little_ice_age"; // autotests start with abraham_lincoln
    bool stopAfterTest = true;
    string initialMode = "testing";

    Dictionary<string, Action> TestDictionary = new Dictionary<string, Action>()
    {
      // not yet used:
      {"fullgame" ,()=> { TestFullGame("testing complete game",
                new List<Action> { ClickProg0, ClickOk, PickResource, ClickOk, ClickPass }); } },
      {"endofage1",()=> { TestEndOfAge("vp at end should be 0,1,2,3,4",new int[] {1,3,4,5,7 },new int[] {0,1,2,3,4 }); } },
      {"endofage2",()=> { TestEndOfAge("vp at end should be 0,0,2,3,4",new int[] {1,1,4,5,7 },new int[] {0,0,2,3,4 }); } },

      //manual tests first, so can batch test
      { "buy_natural", ()=> { TestAction(
        "MANUAL: can I click alhambra when have a wonder? can I perform action exactly one time?", P0,
        ()=> { ProgressCard("mount_kailash",0); },
        new List<Action> { ClickProg0, ClickOk },
        ()=>{ return true; }); }
      },
      {"vesuvius 2", ()=> { TestAction(
        "MANUAL: end of age return worker: no worker available and both kinds of extraworkers checked out", P0,
        ()=> { P0_WOND1("vesuvius"); P0.ExtraWorkers.First().IsCheckedOut = P0.ExtraWorkers.Last().IsCheckedOut = true; P0_MIL(21);  P0.Res.set("worker",0); G.Stats.Round=1;  },
        new List<Action> {  },
        ()=>{ return RD0("worker") == 0;  }); }
      },
      {"wardenclyffe_tower", ()=> { TestAction(
        "MANUAL: wardenclyffe_tower ready: 3 more actions", P0,
        ()=> { P0_WIC("wardenclyffe_tower", 1); P1.HasPassed = false;  },
        new List<Action> { ClickArchitect, ClickOk, ClickWonder1, ClickOk },
        ()=>{ return true; }); }
      },

      // automatic tests: Events: ACHTUNG!!! name des events vor [SPC] wird als name of event card benuetzt!!!!!
      { "aryan_migration", ()=> { TestEventResolution(
        "P0 is least mil and not_least stab: should get 3 wheat and lose 3 book", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.Books = 3; },
        new List<Action> {  },
        ()=>{ return RD0("wheat") == 3 && RD0("book") == -3 ;  }); }
      },
      {"assyrian_deportations", ()=> { TestEventAndWarResolution(
        "P0 regains warloss: 1vp, 6books, only 3 books are regained!", P0,
        ()=> { P1_WAR("first_crusade",6); P0_ADV("buddha"); P0.Books = 3; },
        new List<Action> {  },
        ()=>{ return RD0("vp") == -1 && RD0("book") == 0 ;  }); }
      },
      {"attila", ()=> { TestEventResolution(
        "P0 is least mil and most stab: should lose 3 gold and get 4 coal", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P1.Res.set("wheat",5); },
        new List<Action> {  },
        ()=>{ return RD0("gold") == -3 && RD0("coal") == 4 && RD1("wheat") == -3;  }); }
      },
      {"african_slave_trade", ()=> { TestEventResolution(
        "P0 is least mil and most stab: yes to go last for 6 gold", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.Res.set("wheat",5); },
        new List<Action> { ClickOk },
        ()=>{ return RD0("gold") == 6 && G.Players[0] == P1 && RD0("vp") == -1;  }); }
      },
      {"blackbeard", ()=> { TestEventResolution(
        "P0 is least mil and most stab", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P1.Res.set("wheat",5); },
        new List<Action> {  },
        ()=>{ return RD0("gold") == -5 && RD0("vp") == 1 ;  }); }
      },
      {"bread_and_games", ()=> { TestEventResolution(
        "P0 could trade but decides no,+ 4 coal for stability", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.Res.set("wheat",5); },
        new List<Action> { ClickNo },
        ()=>{ return RD0("coal") == 4;  }); }
      },
      {"columbian_exchange", ()=> { TestEventResolution(
        "P0 is least mil and most stab", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.Res.set("wheat", 5); },
        new List<Action> {  },
        ()=>{ return RD0("wheat") == -3 && RD1("wheat") == 3 && RD1("gold") == -5 ;  }); }
      },
      {"crop_rotation", ()=> { TestEventResolution(
        "P0 is least mil and most stab: P0 undeploys 1 from forum, P1 cannot undeploy from BUILDING!", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0_BM1("forum",2); P1.Res.set("wheat",5); },
        new List<Action> { ClickOk, ClickWorkerCounter1, ClickOk },
        ()=>{ return RD0("wheat") == 6 && RD1("coal") == 0  && RD0("coal") == 2;  }); }
      },
      {"janissaries", ()=> { TestEventResolution(
        "P0 is least mil and most stab", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P1.Res.set("wheat",5); },
        new List<Action> { ClickOk, PickResource, ClickOk, PickResource, ClickOk },
        ()=>{ return RD0("coal") == 5 && RD1("worker") == 2 && RD1("coal") == 4;  },"wheat"); }
      },
      {"hellenism", ()=> { TestEventResolution(
        "P0 is least mil and most stab, P1: undeploy 2 military!!!", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.Res.set("wheat",5); P1.Res.set("wheat",6); },
        new List<Action> { ClickOk, ClickWorkerCounter2,ClickWorkerCounter2,ClickOk },
        ()=>{ return RD0("wheat") == -2 && RD1("book") == 4 && P1.Military == 0 && RD1("coal") == 2;  }); }
      },
      {"little_ice_age", ()=> { TestEventResolution(
        "P0 is least mil and most stab: p1 first gets the books then chooses book, p2 no choice", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P1.Res.set("wheat",5);P0.Res.set("wheat",5); },
        new List<Action> { PickChoice1, ClickOk },
        ()=>{ return RD0("book") == 1 && RD1("wheat") == -3 ;  }); }
      },
      {"magellans_expedition", ()=> { TestEventResolution(
        "P0 is least mil and most stab: p0 hires arch for free", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P1.Res.set("wheat",5); P0_WIC("colosseum", 1); },
        new List<Action> { ClickOk, ClickWonder2,ClickOk },
        ()=>{ return RD0("gold") == 0 && RD0("coal") == 0 && !P0.HasWIC && RD1("gold") == -2;  }); }
      },
      {"pilgrims", ()=> { TestEventResolution(
        "P0 is least mil and most stab, P1 is most wheat takes worker", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P1.Res.set("wheat",5); },
        new List<Action> { PickResource, ClickOk },
        ()=>{ return RD1("gold") == -3 && G.Players[0] == P0 && RD1("worker") == 1 ;  },"stability"); }
      },
      {"sinking_of_the_vasa", ()=> { TestEventResolution(
        "P0 is least mil and least stab: choses to pay gold and wheat", P0,
        ()=> { P1_MIL(6); P1_ADV("buddha"); P0.Res.set("wheat",5); P0.Books=11; },
        new List<Action> { PickChoice0, ClickOk },
        ()=>{ return RD0("gold") == -3 && RD0("wheat") == -5 && RD1("wheat") == 0 && RD1("gold") == 0;  }); }
      },
      {"pax_romana", ()=> { TestEventResolution(
        "P0 is least mil and most stab, P1 1 vp, P0 choice: yes, picks wheat worker", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.Res.set("wheat",5); P1.Res.set("wheat",6); },
        new List<Action> { ClickOk, PickResource, ClickOk },
        ()=>{ return RD1("vp") == 1 && RD0("worker") == 1 && RD0("wheat") == 3; },"wheat"); }
      },
      {"qin_unification", ()=> { TestEventResolution(
        "no one gets vp since no war! P0 is least mil and most stab, P0 should go first", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.Res.set("wheat",5); P1.Res.set("wheat",6); },
        new List<Action> {  },
        ()=>{ return RD1("vp") == 0 && RD0("vp") == 0 && G.Players[0] == P0; },"wheat"); }
      },
      {"qin_unification 2", ()=> { TestEventResolution(
        "P0 is most mil and least stab, P1 should go first", P0,
        ()=> { P1_WAR("first_crusade",6); P0_MIL(9); P1_ADV("buddha");  },
        new List<Action> {  },
        ()=>{ return RD1("vp") == 0 && RD0("vp") == 1 && G.Players[0] == P1; }); }
      },
      {"rigveda", ()=> { TestEventResolution(
        "P0 bought 1 battle and 1 war, no one has most wheat", P0,
        ()=> { P0.CardsBoughtThisRound.Add(Card.MakeCard("first_crusade"));P0.CardsBoughtThisRound.Add(Card.MakeCard("milvian_bridge")); },
        new List<Action> {  },
        ()=>{ return RD0("vp") == 2 && RD0("wheat") == 0 && RD1("vp") == 0 && RD1("wheat") == 0; }); }
      },
      {"sea_peoples", ()=> { TestEventResolution(
        "P0 is least mil and most stab, P1 is most mil and least stab: no one pays vp, P0 must chose to remove or pay: chooses to pay", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0_WIC("colosseum",1); },
        new List<Action> { PickChoice1, ClickOk },
        ()=>{ return RD0("vp") == -1 && RD1("vp") == 0; }); }
      },
      {"shang_oracle_bones", ()=> { TestEventResolution(
        "P0 is most wheat, P1 is least stab", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.Res.set("wheat", 5); P0_WIC("colosseum",1); P1.Books = 5; },
        new List<Action> {  },
        ()=>{ return RD0("gold") == 3 && RD1("book") == -3; }); }
      },
      {"taoism", ()=> { TestEventResolution(
        "P0 is most stab, P1 is pass first", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.HasPassed = P1.HasPassed = true; G.PassOrder = new List<Player> {P1,P0 }; P0.Res.set("wheat", 5); P0_WIC("colosseum",1); P1.Books = 5; },
        new List<Action> {  },
        ()=>{ return RD0("vp") == 1 && RD1("book") == 3; }); }
      },
      {"yellow_turban_rebellion", ()=> { TestEventResolution(
        "P0 is least mil, P1 is least stab", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.Res.set("wheat", 5); P0_WIC("colosseum",1); P1.Books = 5; },
        new List<Action> {  },
        ()=>{ return RD0("wheat") == -3 && RD1("gold") == -1 && G.Players[0] == P0; }); }
      },
      {"benedictine_rule", ()=> { TestEventResolution(
        "P0 is most stab, P1 is least stab", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P1.Res.set("wheat", 8); P0_WIC("colosseum",1); P1.Books = 5; },
        new List<Action> { ClickNo, ClickOk },
        ()=>{ return RD0("wheat") == 4 && RD1("wheat") == -3 && RD1("gold") == 5; }); }
      },
      {"black_death", ()=> { TestEventResolution(
        "P0 is most stab, P1 is most mil, both return a worker", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.Res.set("wheat", 5); P0_WIC("colosseum",1); P1.Books = 5; },
        new List<Action> { },
        ()=>{ return RD0("worker") == -1 && RD1("worker") == -1 && RD1("book") == 3; }); }
      },
      {"chanson_de_roland", ()=> { TestEventResolution(
        "P0 is most stab, P1 is least stab", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P1.Res.set("wheat", 5); P0.Books = 5; },
        new List<Action> {  },
        ()=>{ return RD0("book") == -1 && RD1("book") == 3 && RD1("wheat") == -2 && G.Players[0] == P0; }); }
      },
      {"ecological_collapse", ()=> { TestEventResolution(
        "P0 is most stab, P1 is least stab", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P1.Res.set("wheat", 5); P0_WIC("colosseum",1); P1.Books = 5; },
        new List<Action> { PickChoice1, ClickOk, PickChoice0, ClickOk },
        ()=>{ return RD1("book") == -4 && RD1("wheat") == -2 && G.Players[0] == P1; }); }
      },
      {"feudal_dues", ()=> { TestEventResolution(
        "P0 is most stab, P1 is least stab, P1 loses all gold except 2", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.Res.set("wheat", 5); P0_WIC("colosseum",1); P1.Books = 5; },
        new List<Action> {  },
        ()=>{ return P1.Res.n("gold") == 2; }); }
      },
      {"fourth_crusade", ()=> { TestEventResolution(
        "P0 is most stab, P1 is most mil, p1 buy 1 vp, p0 must pay 4 books, no war so no effect regain loss", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.Books = 5; P0_WIC("colosseum",1); P1.Books = 5; },
        new List<Action> { ClickOk },
        ()=>{ return RD0("book") == -4 && RD1("vp") == 1 && RD1("gold") == -3; }); }
      },
      {"imperial_examination", ()=> { TestEventResolution(
        "P0 is most stab, P1 is most mil,", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P1_ADV("anna_komnene"); },
        new List<Action> {  },
        ()=>{ return RD0("vp") == 1 && RD1("vp") == 1; }); }
      },
      {"martyrdom_of_ali", ()=> { TestEventResolution(
        "P0 is most stab, P1 is most mil", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.Res.set("wheat", 5); P0_WIC("colosseum",1); P1.Books = 5; },
        new List<Action> {  },
        ()=>{ return RD0("vp") == -1 && RD0("book") == 3; }); }
      },
      {"peace_of_god", ()=> { TestEventResolution(
        "P0 is most stab, P1 is most mil", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha");P0_COL0("england");P0_COL1("greenland"); P0_WAR("first_crusade",0); },
        new List<Action> {  },
        ()=>{ return RD0("vp") == 2 && RD1("book") == 4; }); }
      },
      {"raid_on_lindisfarne", ()=> { TestEventResolution(
        "P0 is most stab, P1 is most mil", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.Res.set("wheat", 5); P0_WIC("colosseum",1); P1.Books = 5; },
        new List<Action> {  },
        ()=>{ return RD0("gold") == -4 && RD1("wheat") == -2 && RD1("gold") == -2; }); }
      },
      {"sack_of_baghdad", ()=> { TestEventResolution(
        "P0 is most stab, P1 is most mil", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.Res.set("wheat", 5); P0_WIC("colosseum",1); P0.Books = 5; },
        new List<Action> {  },
        ()=>{ return RD0("book") == -5 && RD0("wheat") == 4; }); }
      },
      {"stupor_mundi", ()=> { TestEventResolution(
        "P0 is most stab, P1 is most mil", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.CardsBoughtThisRound.Add(Card.MakeCard("silk")); },
        new List<Action> {  },
        ()=>{ return RD0("book") == 4 && RD1("gold") == -3; }); }
      },
      {"absolute_monarchy", ()=> { TestEventResolution(
        "P0 is most stab, P1 is most mil", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.Books = 5; },
        new List<Action> {  },
        ()=>{ return RD0("book") == -4 && RD1("gold") == -3; }); }
      },
      
      

      // automatic tests: Progress cards
      {"abraham_lincoln", ()=> { TestAction(
        "pick extra worker", P0,
        ()=> { P0_ADV("abraham_lincoln"); },
        new List<Action> { ClickAdvisor, ClickOk, PickResource, ClickOk, ClickPass },
        ()=>{ return RD0("worker") == 1;  }, "stability"); }
      },
      {"abu_bakr", ()=> { TestAction(
        "p0 gets 2 books from buying battle", P0,
        ()=> { P0_ADV("abu_bakr"); P0_BM0("trireme",2); ProgressCard("milvian_bridge",0); },
        new List<Action> { ClickProg0, ClickOk, PickResource, ClickOk, ClickPass },
        ()=>{ return RD0("book") == 2; }, "wheat"); }
      },
      {"alfred_nobel", ()=> { TestProduction(
        "gets 3 coal foreach worker on industrial building", P0,
        ()=> { P0_BM0("coal_mine",2); P0_BM1("aqueduct",1); P0_ADV("alfred_nobel");  },
        new List<Action> {  },
        ()=>{ return RD0("coal") == 14; }); }
      },
      {"alhambra", ()=> { TestAction(
        "can I click alhambra when have a wonder? can I perform action exactly one time?", P0,
        ()=> { P0_WOND0("alhambra"); P0_WIC("colosseum",1); },
        new List<Action> { ClickWonder0, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD0("military") == 3 && RD0("wheat") == -2 && RD0("coal") == -2; }); }
      },
      {"alhazen", ()=> { TestAction(
        "swap progress cards action", P0,
        ()=> { P0_ADV("alhazen"); P0_BM0("trireme",2); },
        new List<Action> { ClickAdvisor, ClickOk, ClickProg0, ClickProg10, ClickOk, ClickPass },
        ()=>{ return G.Progress.Fields[10].Card.BasicCost == 2; }); }
      },
      {"america_democratic_republicans", ()=> { TestAction(
        "both players have to change dynasty, P0 should get 6 books", P0,
        ()=> { P0_DYN("america_democratic_republicans"); },
        new List<Action> { ClickDynasty, ClickOk, PickChoice0,ClickOk, PickChoice0,ClickOk, ClickPass },
        ()=>{ return true; }); }
      },
      {"america_democratic_republicans_no", ()=> { TestAction(
        "is dyn action possible? should be no", P0,
        ()=> { P0_DYN("america_democratic_republicans");P0_MIL(3);P1.Civ.Dynasties.Clear(); },
        new List<Action> {ClickDynasty, ClickPass },
        ()=>{ return true; }); }
      },
      {"america_dyn1", ()=> { TestAction(
        "new natural wonder ready, plus 2 wheat", P0,
        ()=> { P0_WIC("mount_kailash", 2); },
        new List<Action> { ClickWonder1, ClickOk, ClickPass  },
        ()=>{ return RD0("wheat") == 2; }); }
      },
      {"america_federalist_party", ()=> { TestAction(
        "P0 buys progress building, coal should increase by 2 for dyn", P0,
        ()=> { ProgressCard("forum",0); P0_DYN("america_federalist_party"); },
        new List<Action> { ClickProg0, ClickBM3, ClickOk, ClickPass },
        ()=>{ return RD0("coal") == 2; }); }
      },
      {"angkor_wat", ()=> { TestProduction(
        "if P0 has least military looses 4 books", P0,
        ()=> { P0.Books=5; P0_WOND1("angkor_wat"); P1_MIL(6); },
        new List<Action> { ClickOk, ClickOk },
        ()=>{ return RD0("book") == -4 && RD0("wheat") == 4; }); }
      },
      {"angkor_wat_defaulting", ()=> { TestProduction(
        "if P0 has least military looses 4 books, will default, pay in WHEAT for test to succeed!!!", P0,
        ()=> { P0_WOND1("angkor_wat"); P1_MIL(6); },
        new List<Action> { PickResChoice, ClickOk },
        ()=>{ return RD0("wheat") == 0 && RD0("vp") == -1; },
        "wheat",4); }
      },
      {"anna_komnene", ()=> { TestProduction(
        "no military cost", P0,
        ()=> { P0_ADV("anna_komnene"); P0_BM0("hoplite",4);  },
        new List<Action> {  },
        ()=>{ return RD0("coal") == 0; }); }
      },
      {"arabian_nights", ()=> { TestActionAndProduction(
        "p0 gets 2 books from buying golden age", P0,
        ()=> { ProgressCard("arabian_nights",0); P1.HasPassed = false; },
        new List<Action> { ClickProg0, ClickOk, PickResource, ClickOk, ClickPass, ClickPass },
        ()=>{ return RD0("book") == 2; }, "book"); }
      },
      {"archimedes", ()=> { TestAction(
        "can I click archimedes when have a wonder? can I perform action exactly one time?", P0,
        ()=> { P0_WIC("colosseum", 1); P0_ADV("archimedes"); },
        new List<Action> { ClickAdvisor, ClickOk, ClickWonder1, ClickOk, ClickAdvisor, ClickPass },
        ()=>{ return RD0("military") == 3 && RD0("wheat") == -2 && RD0("coal") == -2; }); }
      },
      {"augustus", ()=> { TestProduction(
        "P0 has most military (because of augustus!) and augustus: should get 2 coal!", P0,
        ()=> { P0_ADV("augustus");  },
        new List<Action> {  },
        ()=>{ return RD0("coal") == 2; }); }
      },
      {"aurora_borealis", ()=> { TestAction(
        "aurora_borealis ready: others pay wheat", P0,
        ()=> { P0_WIC("aurora_borealis");  },
        new List<Action> { ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD0("gold") == 2;  }); }
      },
      {"benjamin_disraeli", ()=> { TestActionAndProduction(
        "P0 should get 8 wheat if bought colony this round", P0,
        ()=> { P0_ADV("benjamin_disraeli"); ProgressCard("armenia",0); P0_MIL(9); },
        new List<Action> { ClickProg0, ClickFieldForColony, ClickOk, ClickPass },
        ()=>{ return RD0("wheat") == 8; }); } //because colony costs 3 and cyrus give 3
      },
      {"big_ben", ()=> { TestProduction(
        "score 1 vp per industrial colony", P0,
        ()=> { P0.InitCiv("america"); P0_COL0("algeria"); P0_WOND0("big_ben");   },
        new List<Action> {  },
        ()=>{ return P0.Score == 10.9; }); }
      },
      {"boudica", ()=> { TestActionAndProduction(
        "P0 deploys military, consequently, boudica is removed", P0,
        ()=> { P0_ADV("boudica"); ProgressCard("hoplite",0); },
        new List<Action> { ClickProg0, ClickBM3, ClickOk, ClickWorker, ClickBM3, ClickOk, ClickPass },
        ()=>{ return P0.Civ.Fields[0].IsEmpty; }); }
      },
      {"boudica II", ()=> { TestActionAndProduction(
        "P0 buys boudica, his military goes up by 1, but it is removed after the action!", P0,
        ()=> { ProgressCard("boudica",0); P0_BM0("hoplite",4); },
        new List<Action> { ClickProg0, ClickOk, ClickPass },
        ()=>{ return P0.Civ.Fields[0].IsEmpty && RD0("gold")==-3; }); }
      },
      {"brandenburg_gate", ()=> { TestAction(
        "brandenburg_gate ready: p1 has most military so only he gets the coal and gold?", P0,
        ()=> { P0_WIC("brandenburg_gate",1);  P1_MIL(3); },
        new List<Action> { ClickArchitect, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD0("gold")==0 && P1.Res.n("gold") > 9;  }); }
      },
      {"british_museum", ()=> { TestAction(
        "british_museum ready: p1 has least military so only he will default in books", P0,
        ()=> { P0_WIC("british_museum",2);  P0_MIL(3); P1.Books = 8; },
        new List<Action> { ClickArchitect, ClickOk, ClickWonder1, ClickOk,PickResChoice, ClickOk, ClickPass },
        ()=>{ return P1.Defaulted.ContainsKey("book");  },"wheat",2); }
      },
      {"cape_of_good_hope", ()=> { TestAction(
        "p0 milmin + 4", P0,
        ()=> { P0_WOND0("cape_of_good_hope"); ProgressCard("armenia",0); P0_MIL(3); },
        new List<Action> { ClickProg0, ClickOk, ClickColony0, ClickOk, ClickPass },
        ()=>{ return RD0("gold") == -3; }); }
      },
      {"carolus_linneaus", ()=> { TestAction(
        "p0 gets additional vp when buying golden and picking vp", P0,
        ()=> { P0_ADV("carolus_linneaus"); ProgressCard("spectacles",0); G.Stats.Age=2; },
        new List<Action> { ClickProg0, ClickOk, PickVP, ClickOk, PickResChoice, ClickOk, ClickPass },
        ()=>{ return RD0("vp") == 2; }, "coal", 2); }
      },
      {"chichen_itza", ()=> { TestAction(
        "p0 gets vp from buying war", P0,
        ()=> { P0_WOND0("chichen_itza"); ProgressCard("first_crusade",0); },
        new List<Action> { ClickProg0, ClickOk, ClickPass },
        ()=>{ return RD0("vp") == 1; }); }
      },
      {"coffee_house", ()=> { TestProduction(
        "P0 has passed last, he should get 4 book", P0,
        ()=> { P0_BM0("coffee_house",2); G.PassOrder = new List<Player> { P1, P0 }; },
        new List<Action> {  },
        ()=>{ return RD0("book") == 4; }); }
      },
      {"china_dyn1", ()=> { TestProduction(
        "P0 has passed first, he should get 1 wheat", P0,
        ()=> { P0_DYN("china_dyn1"); G.PassOrder = new List<Player> { P0, P1 }; },
        new List<Action> {  },
        ()=>{ return RD0("wheat") == 1; }); }
      },
      {"colosseum", ()=> { TestAction(
        "when ready, minus 2 wheat", P1,
        ()=> { P1_WIC("colosseum",1); },
        new List<Action> { ClickArchitect, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD1("military") == 3 && RD1("wheat") == -2 && RD1("coal") == -2; }); }
      },
      {"cyrus_the_great", ()=> { TestActionAndProduction(
        "P0 should get 3 gold if bought colony this round", P0,
        ()=> { P0_ADV("cyrus_the_great"); ProgressCard("armenia",0); P0_MIL(9); },
        new List<Action> { ClickProg0, ClickFieldForColony, ClickOk, ClickPass },
        ()=>{ return RD0("gold") == 0; }); } //because colony costs 3 and cyrus give 3
      },
      {"darwins_voyage", ()=> { TestAction(
        "darwins_voyage ready: 15 gold", P0,
        ()=> { P0_WIC("darwins_voyage", 1);   },
        new List<Action> { ClickArchitect, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD0("gold") == 15; }); }
      },
      {"egypt_dyn1", ()=> { TestAction(
        "same test as archimedes: perform architect action", P1,
        ()=> { P1_WIC("colosseum", 1); },
        new List<Action> { ClickDynasty, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD1("military") == 3 && RD1("wheat") == -2 && RD1("coal") == -2; }); }
      },
      {"egypt_old_kingdom", ()=> { TestProduction(
        "P0 should get 2 times as many books as counters on dyn card", P0,
        ()=> { P0_DYN("egypt_old_kingdom",3);  },
        new List<Action> {  },
        ()=>{ return RD0("book") == 6; }); }
      },
      {"eleanor_of_aquitaine", ()=> { TestActionAndProduction(
        "P0 should get 5 gold if bought colony this round", P0,
        ()=> { P0_ADV("eleanor_of_aquitaine"); ProgressCard("armenia",0); P0_MIL(9); },
        new List<Action> { ClickProg0, ClickFieldForColony, ClickOk, ClickPass },
        ()=>{ return RD0("gold") == 2; }); } //because colony costs 3 and elinor give 5
      },
      {"elizabeth", ()=> { TestWar(
        "P0 is above war because of elizabeth", P0,
        ()=> { P0_ADV("elizabeth"); P1_WAR("first_crusade",6);  },
        new List<Action> {},
        ()=>{ return RD0("vp") == 0 && RD0("book") == 0; }); }
      },
      {"frederic_chopin", ()=> { TestAction(
        "chopin", P0,
        ()=> { ProgressCard("frederic_chopin",0); P1.HasPassed = false; },
        new List<Action> { ClickProg0, ClickOk, ClickSpecialOption0, ClickOk, ClickPass,ClickPass },
        ()=>{ return true; }); }
      },
      {"forbidden_palace", ()=> { TestProduction(
        "P0 has passed first, he should get 1 wheat", P0,
        ()=> { P0_WOND0("forbidden_palace"); G.PassOrder = new List<Player> { P0, P1 }; },
        new List<Action> {  },
        ()=>{ return RD0("vp") == 1; }); }
      },
      {"galileo_galilei", ()=> { TestAction(
        "P0 can pick wonder or golden for free", P0,
        ()=> { ProgressCard("silk", 0); P0_ADV("galileo_galilei"); },
        new List<Action> { ClickAdvisor, ClickOk, ClickProg0, ClickOk, PickResource, ClickOk, ClickPass },
        ()=>{ return RD0("coal") == 2 && RD0("gold") == 0; }, "coal"); }
      },
      {"grand_canyon", ()=> { TestProduction(
        "score 1 vp per natural wonder", P0,
        ()=> { P0.InitCiv("america"); P0_WOND0("grand_canyon"); P0_WOND1("cape_of_good_hope");   },
        new List<Action> {  },
        ()=>{ return P0.Score == 9.4; }); }
      },
      {"grand_duchy_of_finland", ()=> { TestAction(
        "skip turn", P0,
        ()=> { P0_COL0("grand_duchy_of_finland"); },
        new List<Action> { ClickColony0, ClickOk, ClickPass },
        ()=>{ return true; }); }
      },
      {"great_barrier_reef", ()=> { TestAction(
        "trade 1 vp for 5 wheat", P0,
        ()=> { P0_WOND0("great_barrier_reef"); },
        new List<Action> { ClickWonder0, ClickOk, ClickPass },
        ()=>{ return RD0("wheat") == 5 && RD0("vp") == -1; }); }
      },
      {"great_lighthouse", ()=> { TestAction(
        "buy card for 3 gold gives 1 book", P0,
        ()=> { P0_WOND1("great_lighthouse"); ProgressCard("hoplite",0);},
        new List<Action> {ClickProg0, ClickBM3, ClickOk, ClickPass},
        ()=>{ return RD0("book") == 1 && RD0("gold") == -3; }); }
      },
      {"great_lighthouse 2", ()=> { TestAction(
        "buy card for 3 gold but great_lighthouse not ready yet! should NOT give a book!!!", P0,
        ()=> { P0_WIC("great_lighthouse"); ProgressCard("hoplite",0);},
        new List<Action> {ClickProg0, ClickBM3, ClickOk, ClickPass},
        ()=>{ return RD0("book") == 0 && RD0("gold") == -3; }); }
      },
      {"great_library", ()=> { TestAction(
        "test for golden age bonus and should be removed for least stability", P0,
        ()=> { P0_WOND0("great_library"); ProgressCard("spectacles",0); },
        new List<Action> { ClickProg0, ClickOk, PickResource, ClickOk, ClickPass },
        ()=>{ return RD0("coal") == 3; }, "coal"); }
      },
      {"great_wall", ()=> { TestWar(
        "pass first, therefore no vp lost to war, still looses resources!", P0,
        ()=> { P0_WOND0("great_wall"); P1_WAR("first_crusade",6); P0.Books=6; G.PassOrder = new List<Player> { P0, P1 }; },
        new List<Action> {},
        ()=>{ return RD0("vp") == 0 && RD0("book") == -4; }); } //because great wall gives 2 stability!
      },
      {"hanging_gardens", ()=> { TestProduction(
        "give stability, production 2 wheat, -1 coal", P0,
        ()=> { P0_WOND1("hanging_gardens");  },
        new List<Action> {  },
        ()=>{ return RD0("wheat") == 2 && RD0("coal")==-1; }); }
      },
      {"hannibal", ()=> { TestAction(
        "pay 1 more for battle because other has hannibal!", P0,
        ()=> { P1_ADV("hannibal");P0_BM0("hoplite",1); ProgressCard("milvian_bridge",0);},
        new List<Action> {ClickProg0, ClickOk, PickResource, ClickOk, ClickPass},
        ()=>{ return RD0("gold") == -4; },"coal"); }
      },
      {"harald_hardrada", ()=> { TestAction(
        "remove advisor since least military", P0,
        ()=> { P0_ADV("harald_hardrada");P1_MIL(3); P0_MIL(6);ProgressCard("forum",0);},
        new List<Action> { ClickProg0, ClickBM2, ClickOk, ClickPass},
        ()=>{ return NO_ADVISOR(P0); }); }
      },
      {"hatshepsut", ()=> { TestAction(
        "wonder ready get 3 books", P0,
        ()=> { P0_ADV("hatshepsut"); P0_WIC("hanging_gardens",1); },
        new List<Action> { ClickArchitect, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD0("book") == 3; }); }
      },
      {"hawaii", ()=> { TestAction(
        "hawaii ready: pick worker place cross on track", P0,
        ()=> { P0_WIC("hawaii");  },
        new List<Action> { ClickWonder1, ClickOk, PickResource,ClickOk,ClickPass },
        ()=>{ return true;  },"wheat"); }
      },
      {"himegi_castle", ()=> { TestAction(
        "p0 gets 4 coal", P0,
        ()=> { P0_WOND0("himegi_castle"); ProgressCard("hammam",0); },
        new List<Action> { ClickProg0, ClickBM0, ClickOk, ClickPass },
        ()=>{ return RD0("coal") == 4; }); }
      },
      {"hypatia", ()=> { TestActionAndProduction(
        "test if goes away when stability goes over 2, should NOT produce any books!", P0,
        ()=> { P0_ADV("hypatia"); ProgressCard("tibet",0);  },
        new List<Action> { ClickProg0, ClickColony0, ClickOk, ClickPass },
        ()=>{ return !P0.HasAdvisor && RD0("book") == 0;  }); }
      },
      {"isabella", ()=> { TestProduction(
        "gets 6 gold 3 foreach renaissance colony", P0,
        ()=> { P0_COL0("brazil"); P0_COL1("philippines"); P0_ADV("isabella");  },
        new List<Action> {  },
        ()=>{ return RD0("gold") == 6; }); }
      },
      {"le_vite", ()=> { TestAction(
        "p0 buys le vite and picks the weired effect: arch should cost 1 less now!", P0,
        ()=> { ProgressCard("le_vite",0); P0_WIC("colosseum",0); },
        new List<Action> { ClickProg0, ClickOk, PickChoice0, ClickOk, ClickArchitect, ClickOk, ClickPass },
        ()=>{ return RD0("coal") == -1; }, "coal", 2); }
      },
      {"lin_zexu", ()=> { TestProduction(
        "if P0 has least military looses 4 books", P0,
        ()=> { P0.Books=15; P0_ADV("lin_zexu"); P1_MIL(6);  },
        new List<Action> {  },
        ()=>{ return RD0("book") == -8; }); }
      },
      {"mansa_musa", ()=> { TestAction(
        "spends last gold", P0,
        ()=> { P0_ADV("mansa_musa"); ProgressCard("forum",0); P0_GOLD(3); },
        new List<Action> { ClickProg0, ClickBM2, ClickOk, ClickPass},
        ()=>{ return RD0("gold") == -3 && RD0("book")==2 && RD0("wheat")==1; }); }
      },
      {"marie_curie", ()=> { TestAction(
        "can I click marie_curie when have a wonder? can I perform action exactly twice time?", P0,
        ()=> { P0_WIC("colosseum", 0); P0_ADV("marie_curie"); },
        new List<Action> { ClickAdvisor, ClickOk, ClickAdvisor, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD0("military") == 3 && RD0("wheat") == -2 && RD0("coal") == -4; }); }
      },
      {"marco_polo", ()=> { TestAction(
        "trade 2 wheat or coal for 4 gold", P0,
        ()=> { P0_ADV("marco_polo"); },
        new List<Action> { ClickAdvisor, ClickOk, PickResource, ClickOk, ClickPass},
        ()=>{ return RD0("coal")==-2 && RD0("gold") == 4; },"coal"); }
      },
      {"martin_luther", ()=> { TestWar(
        "add 4 wheat to defeated resources", P0,
        ()=> { P1_ADV("martin_luther"); P1_WAR("first_crusade",6); P0.Books=8; },
        new List<Action> {},
        ()=>{ return RD0("vp") == -2 && RD0("book") == -8 && RD0("wheat") == -2; },"gold",2); }
      },
      {"mit", ()=> { TestAction(
        "deploy 1 worker when buy building", P0,
        ()=> { P0_WOND0("mit"); Progress0("ziggurat"); P0_BM0("forum",5);P0.Res.set("worker",0); },
        new List<Action> { ClickProg0, ClickBM2, ClickOk, ClickOk, ClickBM0, ClickOk, ClickPass },
        ()=>{ return true; }); }
      },
      {"moai_statues", ()=> { TestAction(
        "moai_statues ready: 12 coal", P0,
        ()=> { P0_WIC("moai_statues", 2);   },
        new List<Action> { ClickArchitect, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD0("coal") == 11; }); }
      },
      {"montezuma", ()=> { TestAction(
        "p0 gets 3 books from buying battle", P0,
        ()=> { P0_ADV("montezuma"); ProgressCard("mughal_invasion",0); },
        new List<Action> { ClickProg0, ClickOk, ClickPass },
        ()=>{ return RD0("book") == 3; }); }
      },
      {"mount_ararat", ()=> { TestAction(
        "mount_ararat ready: others pay wheat", P0,
        ()=> { P0_WIC("mount_ararat",2);  P1.Res.set("wheat",2); P1.Books=2; },
        new List<Action> { ClickWonder1, ClickOk, ClickPass },
        ()=>{ return P1.Res.n("wheat") == 0 && P1.Books==0;  }); }
      },
      {"niccolo_machiavelli", ()=> { TestNewEvent(
        "Choose out of two cards", P0,
        ()=> { Args = new object[] { G.Stats.PeekEventCards(2)[0].Name };  P0_ADV("niccolo_machiavelli");   },
        new List<Action> {PickChoice0,ClickOk, ClickPass},
        ()=>{ return G.Stats.EventCard.Name == Args[0] as string; }); }
      },
      {"northwest_passage", ()=> { TestAction(
        "ready: return 2 workers", P0,
        ()=> { P0_WIC("northwest_passage"); P0_MIL(15);P0.Res.set("worker",0); },
        new List<Action> { ClickWonder1, ClickOk, ClickPass },
        ()=>{ return P0.Military == 9;  }); }
      },
      {"notre_dame", ()=> { TestProduction(
        "P0 should get 3 book if most stability", P0,
        ()=> { P0_WOND0("notre_dame"); P0_STAB(3); },
        new List<Action> {  },
        ()=>{ return RD0("book") == 5; }); }
      },
      {"peter_the_great", ()=> { TestProduction(
        "Peter the great pick resource for military > war", P0,
        ()=> { P0_ADV("peter_the_great"); P1_WAR("first_crusade", 6); P0_BM0("hoplite",3);  },
        new List<Action> { PickResource, ClickOk },
        ()=>{ return RD0("book") == 5; },"book"); }
      },
      {"petra", ()=> { TestAction(
        "trade 1 wheat for 3 of other resource", P0,
        ()=> { P0_WOND1("petra"); },
        new List<Action> { ClickWonder1,ClickOk,PickResource,ClickOk, ClickPass},
        ()=>{ return RD0("wheat")==-1 && RD0("book") == 3; },"book"); }
      },
      {"piazza_san_marco", ()=> { TestAction(
        "trade 2 gold for 5 of other resource", P0,
        ()=> { P0_WOND1("piazza_san_marco"); },
        new List<Action> { ClickWonder1,ClickOk,PickResource,ClickOk, ClickPass},
        ()=>{ return RD0("gold")==-2 && RD0("coal") == 5; },"coal"); }
      },
      {"porcelain_tower", ()=> { TestAction(
        "P0 buys progress card advisor, should be able to place it on porcelain tower", P0,
        ()=> { ProgressCard("archimedes",0); P0_WOND0("porcelain_tower"); },
        new List<Action> { ClickProg0, ClickWonder0, ClickOk, ClickPass },
        ()=>{ return P0.Civ.Fields[9].Card.Name == "archimedes"; }); }
      },
      {"potala_palace", ()=> { TestProduction(
        "score 1 vp per advisor", P0,
        ()=> { P0.InitCiv("america"); P0_WOND0("potala_palace"); P0_ADV("archimedes"); P0_BM0("hoplite",3);  },
        new List<Action> {  },
        ()=>{ return P0.Score == 11.1; }); }
      },
      {"qin_shi_huang", ()=> { TestAction(
        "least stability remove, P0 loses stability", P0,
        ()=> { P0_ADV("qin_shi_huang"); P0_Deploy(4,1); P1_ADV("saint_augustine");Progress0("forge");  },
        new List<Action> { ClickProg0, ClickBM3, ClickOk, ClickPass },
        ()=>{ return P0.Civ.Fields[CardType.iADV].IsEmpty;  }); }
      },
      {"red_fort", ()=> { TestAction(
        "buy card for 1 gold givesc 2 book", P0,
        ()=> { P0_WOND1("red_fort"); ProgressCard("hoplite",15);},
        new List<Action> {ClickProg15, ClickBM3, ClickOk, ClickPass},
        ()=>{ return RD0("book") == 2 && RD0("gold") == -1; }); }
      },
      {"royal_society", ()=> { TestAction(
        "deploy 1 worker for free action", P0,
        ()=> { P0_WOND0("royal_society"); P0_BM0("trireme",2); },
        new List<Action> { ClickWonder0, ClickOk, ClickBM0, ClickOk, ClickPass },
        ()=>{ return RD0("coal")==0 && P0.Military == 9; }); }
      },
      {"sahara", ()=> { TestWar(
        "P0 is above war because of sahara", P0,
        ()=> { P0_WOND0("sahara"); P1_WAR("first_crusade",3);  },
        new List<Action> {},
        ()=>{ return RD0("vp") == 0 && RD0("book") == 0; }); }
      },
      {"saint_augustine", ()=> { TestProduction(
        "P0 should get 2 book if most stability", P0,
        ()=> { P0_ADV("saint_augustine");  },
        new List<Action> {  },
        ()=>{ return RD0("book") == 2; }); }
      },
      {"sankore_university", ()=> { TestAction(
        "sankore_university ready: 8 books", P0,
        ()=> { P0_WIC("sankore_university", 2);   },
        new List<Action> { ClickArchitect, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD0("book") == 8; }); }
      },
      {"sejong_the_great", ()=> { TestAction(
        "p0 gets 2 coal from buying golden", P0,
        ()=> { P0_ADV("sejong_the_great"); ProgressCard("spectacles",0); },
        new List<Action> { ClickProg0, ClickOk, PickResource, ClickOk, ClickPass },
        ()=>{ return RD0("coal") == 4; }, "coal"); }
      },
      {"shaka_zulu", ()=> { TestAction(
        "p0 buys colony, others all get 5 coal!", P0,
        ()=> { P0_ADV("shaka_zulu"); ProgressCard("armenia",0); P0_MIL(12); },
        new List<Action> { ClickProg0, ClickColony0, ClickOk, ClickPass },
        ()=>{ return P1.Res.n("coal")>=10; }); }
      },
      {"shwedagon_pagoda", ()=> { TestAction(
        "p0 as long as no one passed can increase stab by 1 each action!", P0,
        ()=> { P0_WOND0("shwedagon_pagoda"); P1.HasPassed = false; },
        new List<Action> { ClickWonder0, ClickOk, ClickWorker, ClickBM0, ClickOk, ClickWonder0, ClickOk, ClickPass, ClickPass },
        ()=>{ return P0.Stability == 2; }); }
      },
      {"siberia", ()=> { TestAction(
        "siberia ready: cover additional field", P0,
        ()=> { P0_WIC("siberia");  },
        new List<Action> { ClickWonder1, ClickOk, ClickWonder2, ClickOk, ClickPass },
        ()=>{ return true;  }); }
      },
      {"sistine_chapel", ()=> { TestAction(
        "get 3 books when hire architect", P0,
        ()=> { P0_WOND0("sistine_chapel"); P0_WIC("colosseum", 1); P0_ADV("archimedes"); },
        new List<Action> { ClickAdvisor, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD0("book") == 3 && RD0("military") == 3 && RD0("wheat") == -2 && RD0("coal") == -2; }); }
      },
      {"simon_bolivar", ()=> { TestAction(
        "remove colony get resources", P0,
        ()=> { P0_ADV("simon_bolivar"); P0_COL0("armenia");P0_COL1("algeria"); },
        new List<Action> { ClickAdvisor, ClickOk, ClickColony0, ClickOk, ClickPass },
        ()=>{ return RD0("gold") == 4 && RD0("coal") == 4;  }, "armenia"); }
      },
      {"siwa_oasis", ()=> { TestAction(
        "p0 gets books from buying colony", P0,
        ()=> { P0_WOND0("siwa_oasis"); ProgressCard("nubia",0); P0_MIL(6); },
        new List<Action> { ClickProg0, ClickOk, ClickFieldForColony, ClickOk, ClickPass },
        ()=>{ return RD0("book") == 3; }); }
      },
      {"spice_islands", ()=> { TestGrowth(
        "P0 should get 8 gold instead of 4", P0,
        ()=> { P0_WOND0("spice_islands");  },
        new List<Action> { PickResource,ClickOk,PickResource,ClickOk },
        ()=>{ return RD0("gold") == 8; },"gold"); }
      },
      {"solomons_temple", ()=> { TestAction(
        "end of age + 1 vp", P0,
        ()=> { P0_WOND1("solomons_temple"); G.Stats.Round=1;  },
        new List<Action> { ClickPass },
        ()=>{ return RD0("vp") == 1;  }); }
      },
      {"south_pole_expedition", ()=> { TestAction(
        "south_pole_expedition ready: pay wheat", P0,
        ()=> { P0_WIC("south_pole_expedition", 1);  P0.Res.set("wheat",5); },
        new List<Action> { ClickArchitect, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD0("wheat") == -5; }); }
      },
      {"sphinx", ()=> { TestAction(
        "new wonder ready +5 coal, should get 3 coal", P0,
        ()=> { P0_WOND0("sphinx"); P0_WIC("colosseum", 1);  },
        new List<Action> { ClickArchitect, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD0("coal") == 3;  }); }
      },
      {"sphinx 2", ()=> { TestAction(
        "sphinx ready: nothing!", P0,
        ()=> { P0_WIC("sphinx",1);   },
        new List<Action> { ClickArchitect, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD0("coal") == -2;  }); }
      },
      {"statue_of_liberty", ()=> { TestProduction(
        "score 2 vp if most stability", P0,
        ()=> { P0.InitCiv("america"); P0_WOND0("statue_of_liberty"); P0_ADV("buddha"); P0_BM0("hoplite",3);  },
        new List<Action> {  },
        ()=>{ return P0.Score == 12.4; }); }
      },
      {"stonehenge", ()=> { TestAction(
        "stonehenge ready: rewards", P0,
        ()=> { P0_WIC("stonehenge",2);   },
        new List<Action> { ClickArchitect, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD0("book") == 6 && RD0("wheat") == 4;  }); }
      },
      {"suleiman", ()=> { TestAction(
        "most military: pick extra worker", P0,
        ()=> { P0_ADV("suleiman"); P0_MIL(6); },
        new List<Action> { ClickAdvisor, ClickOk, PickResource, ClickOk, ClickPass },
        ()=>{ return RD0("worker") == 1;  }, "stability"); }
      },
      {"sun_tzu", ()=> { TestAction(
        "double turn, able to take 2 architects!", P0,
        ()=> { P0_ADV("sun_tzu"); P0_WIC("stonehenge",1); G.Stats.Architects = 3; },
        new List<Action> { ClickArchitect, ClickOk, ClickArchitect, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD0("book") == 6 && RD0("wheat") == 4;  }); }
      },
      {"taj_mahal", ()=> { TestAction(
        "taj_mahal ready: 15 books", P0,
        ()=> { P0_WIC("taj_mahal",2); },
        new List<Action> { ClickArchitect, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD0("book") == 15;  }); }
      },
      {"terracotta_army", ()=> { TestAction(
        "terracotta_army ready: player 1 cannot pay 4 gold! he pays 2 gold, 1 vp and 2 books?", P0,
        ()=> { P0_WIC("terracotta_army",2);  P1.Res.set("gold",2); P1.Books=2; P0_STAB(3); },
        new List<Action> { ClickArchitect, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return P1.Res.n("gold") == 0 && P1.Books==0;  }); }
      },
      {"the_pillar_of_hercules", ()=> { TestAction(
        "7 military for this round", P0,
        ()=> { P0_WIC("the_pillar_of_hercules",0);  },
        new List<Action> { ClickWonder1, ClickOk, ClickPass },
        ()=>{ return P0.Military == 7;  }); }
      },
      {"thomas_aquino", ()=> { TestProduction(
        "if P0 has most stability gets 4 books", P0,
        ()=> { P0_ADV("thomas_aquino");  },
        new List<Action> {  },
        ()=>{ return RD0("book") == 4; }); }
      },
      {"titusville", ()=> { TestProduction(
        "score coal resource double", P0,
        ()=> { P0.InitCiv("america"); P0_WOND0("titusville");  },
        new List<Action> {  },
        ()=>{ return P0.Score == 7.9; }); }
      },
      {"assassin", ()=> { TestAction(
        "buy battle: others pay wheat or remove advisor", P0,
        ()=> { P0_ADV("abu_bakr"); P0_BM0("trireme",2); P0_BM0("assassin",2); ProgressCard("milvian_bridge",0); P1.Res.set("wheat",4); },
        new List<Action> { ClickProg0, ClickOk, PickResource, ClickOk, ClickPass },
        ()=>{ return P1.Res.n("wheat") == 1;  },"book"); }
      },
      {"titanic", ()=> { TestAction(
        "titanic ready: player 0 picks remove advisor", P0,
        ()=> { P0_WIC("titanic",2); P0_ADV("archimedes"); P1_ADV("archimedes"); },
        new List<Action> { ClickArchitect, ClickOk, ClickWonder1, ClickOk, PickChoice1, ClickOk, PickChoice0, ClickOk, ClickPass },
        ()=>{ return !P0.HasAdvisor && RD0("gold") == 0;  }); }
      },
      {"tokugawa", ()=> { TestProduction(
        "gets 4 coal, 2 foreach renaissance building and military", P0,
        ()=> { P0_BM0("sawmill"); P0_BM1("shipyard"); P0_ADV("tokugawa");  },
        new List<Action> {  },
        ()=>{ return RD0("coal") == 4; }); }
      },
      {"trireme", ()=> { TestActionAndProduction(
        "does it correctly charge 2 per deploy and none in production?", P0,
        ()=> { P0_BM1("trireme"); },
        new List<Action> { ClickWorker, ClickBM1, ClickOk, ClickPass },
        ()=>{ return RD0("coal") == -2; }); }
      },
      {"uluru", ()=> { TestAction(
        "uluru ready: vp per player passed", P0,
        ()=> { P0_WIC("uluru",2);   },
        new List<Action> { ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD0("vp") == 1;  }); }
      },
      {"uncle_toms_cabin", ()=> { TestAction(
        "remove all buildings deploy cost 1", P0,
        ()=> { ProgressCard("uncle_toms_cabin",0); P0_MIL(12); },
        new List<Action> { ClickProg0, ClickOk, PickChoice0, ClickOk, ClickPass },
        ()=>{ return true; }, "coal", 2); }
      },
      {"uraniborg", ()=> { TestAction(
        "p0 gets 4 coal from buying golden", P0,
        ()=> { P0_WOND0("uraniborg"); ProgressCard("spectacles",0); },
        new List<Action> { ClickProg0, ClickOk, PickResource, ClickOk, ClickPass },
        ()=>{ return RD0("coal") == 4; }, "coal"); }
      },
      {"varanasi", ()=> { TestProduction(
        "P0 should get 1 book and 1 wheat for every undeployed worker", P0,
        ()=> { P0_WOND1("varanasi");  },
        new List<Action> {  },
        ()=>{ return RD0("book") == 5 && RD0("wheat") == 5; }); }
      },
      {"versailles", ()=> { TestAction(
        "make sure least stability shows correctly", P0,
        ()=> { P0_WIC("versailles",2); G.Stats.Architects = 3; },
        new List<Action> { ClickArchitect, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return P0.Stability == -100;  }); }
      },
      {"vesuvius", ()=> { TestAction(
        "end of age return worker", P0,
        ()=> { P0_WOND1("vesuvius"); G.Stats.Round=1;  },
        new List<Action> { ClickPass },
        ()=>{ return RD0("worker") == -1;  }); }
      },
      {"zhu_xi", ()=> { TestActionAndProduction(
        "P0 replaces zhu_xi: extra advisor should come!", P0,
        ()=> { ProgressCard("boudica",0); P0_ADV("zhu_xi");  },
        new List<Action> { ClickProg0, ClickOk, ClickPass },
        ()=>{ return RD0("stability") == 0; }); }
      },

    };


  }
}

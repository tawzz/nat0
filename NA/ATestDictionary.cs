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
    string startWithTest = "america_dyn1"; // autotests start with america_dyn1
    bool stopAfterTest = false;
    string initialMode = "testing";

    Dictionary<string, Action> TestDictionary = new Dictionary<string, Action>()
    {
      // dynasties
      {"america_dyn1", ()=> { TestAction(
        "new natural wonder ready, plus 2 wheat", P0,
        ()=> { P0_DYN("america_dyn1");P0_WIC("mount_kailash", 2); },
        new List<Action> { ClickWonder1, ClickOk, ClickPass  },
        ()=>{ return RD0("wheat") == 2; }); }
      },
      {"america_democratic_republicans", ()=> { TestAction( // p0 darf nicht mit america init in dem test weil sonst kein choice von dyn!
        "both players have to change dynasty, P0 should get 6 books", P0,
        ()=> { P0.Reset(true,"japan"); P0_DYN("america_democratic_republicans"); },
        new List<Action> { ClickDynasty, ClickOk, PickChoice0, ClickOk, PickChoice0, ClickOk, ClickPass },
        ()=>{ return RD0("book") == 6; }); }
      },
      {"america_democratic_republicans_no", ()=> { TestAction(
        "is dyn action possible? should be no", P0,
        ()=> { P0_DYN("america_democratic_republicans");P0_MIL(3);P1.Civ.Dynasties.Clear(); },
        new List<Action> {ClickDynasty, ClickPass },
        ()=>{ return true; }); }
      },
      {"america_federalist_party", ()=> { TestAction(
        "P0 buys progress building, coal should increase by 2 for dyn", P0,
        ()=> { ProgressCard("forum",0); P0_DYN("america_federalist_party"); },
        new List<Action> { ClickProg0, ClickBM3, ClickOk, ClickPass },
        ()=>{ return RD0("coal") == 2; }); }
      },
      {"arabia_dyn1", ()=> { TestAction(
        "p0 checks out extra worker from buying battle", P0,
        ()=> { P0_DYN("arabia_dyn1"); P0_MIL(6); ProgressCard("milvian_bridge",0); },
        new List<Action> { ClickProg0, ClickOk, ClickOk, PickResource, ClickOk, PickResource, ClickOk, ClickPass },
        ()=>{ return RD0("worker") == 1; }, "wheat"); }
      },
      {"arabia_abbasid_caliphate", ()=> { TestAction(
        "P0 buys ga, p1 gets to buy vp and gets additional vp from carolus linneaus", P0,
        ()=> { P1.Reset(true,"arabia"); P1_DYN("arabia_abbasid_caliphate"); P1_ADV("carolus_linneaus"); ProgressCard("silk", 0);  },
        new List<Action> { ClickProg0, ClickOk, ClickOk, PickResChoice, ClickOk, PickResource, ClickOk, ClickPass },
        ()=>{ return RD1("vp") == 2; }, "coal",1); }
      },
      {"arabia_umayyad_caliphate", ()=> { TestActionAndProduction(
        "P0 has milmin bonus of 4", P0,
        ()=> { P0_DYN("arabia_umayyad_caliphate"); ProgressCard("nubia",0);  },
        new List<Action> { ClickProg0, ClickFieldForColony, ClickOk, ClickPass },
        ()=>{ return P0.HasColony; }); }
      },
      {"china_dyn1", ()=> { TestProduction(
        "P0 has passed first, he should get 1 wheat", P0,
        ()=> { P0_DYN("china_dyn1"); G.PassOrder = new List<Player> { P0, P1 }; },
        new List<Action> {  },
        ()=>{ return RD0("wheat") == 1; }); }
      },
      {"china_ming_dynasty", ()=> { TestGrowth(
        "P0 should get 4 wheat with 1 worker", P0,
        ()=> { P0_DYN("china_ming_dynasty");  },
        new List<Action> { PickResource,ClickOk,PickFirstResource,ClickOk,PickFirstResource,ClickOk },
        ()=>{ return RD0("worker") == 1 && RD0("wheat") == 4; },"worker"); }
      },
      {"china_qin_dynasty", ()=> { TestAction(
        "return worker for free architect", P0,
        ()=> { P0_DYN("china_qin_dynasty");P0_WIC("colosseum", 1); },
        new List<Action> { ClickDynasty, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD0("worker") == -1 && !P0.HasWIC && RD0("coal") == 0; }); }
      },
      {"egypt_dyn1", ()=> { TestAction(
        "same test as archimedes: perform architect action", P0,
        ()=> { P0.Reset(true,"egypt"); P0_WIC("colosseum", 1); },
        new List<Action> { ClickDynasty, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD0("military") == 3 && RD0("wheat") == -2 && RD0("coal") == -2; }); }
      },
      {"egypt_old_kingdom prod", ()=> { TestProduction(
        "P0 should get 2 times as many books as counters on dyn card", P0,
        ()=> { P0_DYN("egypt_old_kingdom",3);  },
        new List<Action> {  },
        ()=>{ return RD0("book") == 6; }); }
      },
      {"egypt_old_kingdom action", ()=> { TestAction(
        "action: place counter on dyn card", P0,
        ()=> { P0_DYN("egypt_old_kingdom",3); P0_ADV("archimedes");  },
        new List<Action> { ClickDynasty, ClickOk, ClickPass },
        ()=>{ return P0.GetCard("egypt_old_kingdom").NumDeployed == 4 && !P0.HasRemovableAdvisor;}); }
      },
      {"egypt_old_kingdom prod if defeated", ()=> { TestProduction(
        "prod if defeated: remove 1 counter", P0,
        ()=> { P0_DYN("egypt_old_kingdom",3); P0_ADV("archimedes"); P1_WAR(); P0.Books = 10; },
        new List<Action> {  },
        ()=>{ return P0.GetCard("egypt_old_kingdom").NumDeployed == 2 && G.Stats.IsWar;}); }
      },
      {"egypt_new_kingdom", ()=> { TestAction(
        "p0 pays 2 less for buying battle, test combination w/ abu", P0,
        ()=> { P0.Reset(true,"egypt"); P0_ADV("abu_bakr"); P0_DYN("egypt_new_kingdom"); P0_BM0("trireme",2); ProgressCard("milvian_bridge",0); },
        new List<Action> { ClickProg0, ClickOk, PickResource, ClickOk, ClickPass },
        ()=>{ return RD0("book") == 2 && RD0("gold") == -1; }, "wheat"); }
      },
      {"ethiopia_dyn1", ()=> { TestOrder(
        "p0 goes first because of stability", P0,
        ()=> { P0_DYN("ethiopia_dyn1"); P1_MIL(3); P0_ADV("buddha");P0_WOND0("hanging_gardens"); },
        new List<Action> {  },
        ()=>{ return G.Players[0] == P0; }); }
      },
      {"ethiopia_axumite_kingdom", ()=> { TestNewEventAndAction(
        "before actions, p0 marks a progress card, buyer has to pay 3 gold to p0", P0,
        ()=> { P0_DYN("ethiopia_axumite_kingdom"); ProgressCard("forum",0); P1.HasPassed = false; P1.Res.set("gold",10); },
        new List<Action> { ClickProg0, ClickOk, ClickPass, ClickProg0, ClickBM2, ClickOk, ClickPass },
        ()=>{ return RD0("gold") == 3 && RD1("gold") == -6; },false); }
      },
      {"ethiopia_sheba", ()=> { TestActionAndProduction(
        "no military cost if bought colony", P0,
        ()=> { P0_DYN("ethiopia_sheba"); P0_MIL("hoplite",2); ProgressCard("nubia",0);  },
        new List<Action> {ClickProg0, ClickFieldForColony, ClickOk, ClickPass },
        ()=>{ return RD0("coal") == 0; }); }
      },
      {"mongolia_dyn1", ()=> { TestWar(
        "P0 defeated, P1 has mongolia dyn, P0 pays 2 more resources (8 books instead of 6)", P0,
        ()=> { P1_DYN("mongolia_dyn1"); P1_WAR("first_crusade", 6); P0.Books = 10; },
        new List<Action> {},
        ()=>{ return RD0("vp") == -1 && RD0("book") == -8; }); }
      },
      {"mongolia_golden_horde", ()=> { TestAction(
        "others buy war: gold goes to owner", P0,
        ()=> { P1_DYN("mongolia_golden_horde"); ProgressCard("first_crusade",0);  },
        new List<Action> {ClickProg0, ClickOk, ClickPass },
        ()=>{ return RD1("gold") == 3; }); }
      },
      {"mongolia_yuan_dynasty", ()=> { TestAction(
        "when played, check out 3 workers", P0,
        ()=> { P0.Reset(true,"mongolia");   },
        new List<Action> { ClickTurmoil, ClickOk, PickChoice1, ClickOk, PickFirstResource, ClickOk, PickFirstResource, ClickOk, PickFirstResource, ClickOk,ClickPass },
        ()=>{ return RD0("worker") == 3; }); }
      },
      {"greece_dyn1", ()=> { TestAction(
        "golden age bonus", P0,
        ()=> { P0_DYN("greece_dyn1"); ProgressCard("silk",0);  },
        new List<Action> {ClickProg0, ClickOk, PickFirstResource, ClickOk, ClickPass },
        ()=>{ return RD0("coal") == 3; }); }
      },
      {"greece_athens", ()=> { TestProduction(
        "P0 should get 3 gold if most book", P0,
        ()=> { P0_DYN("greece_athens"); P0_WOND0("notre_dame"); P0_STAB(3); },
        new List<Action> {  },
        ()=>{ return RD0("book") == 5 && RD0("gold") == 3; }); }
      },
      {"greece_sparta", ()=> { TestAction(
        "exactly 1 military deployed: +4 military", P0,
        ()=> { P0_DYN("greece_sparta"); P0_MIL(3); },
        new List<Action> { ClickPass },
        ()=>{ return P0.Military == 7; }); }
      },
      {"india_dyn1", ()=> { TestProduction(
        "P0 should get 2 wheat", P0,
        ()=> { P0.Reset(true,"india"); P0_WOND0("notre_dame"); P0_STAB(3); },
        new List<Action> {  },
        ()=>{ return RD0("wheat") == 2; }); }
      },
      {"india_mauryan_empire", ()=> { TestGrowth(
        "P0 takes worker, may take 2 more: yes", P0,
        ()=> { P0_DYN("india_mauryan_empire");  },
        new List<Action> { PickResource,ClickOk,PickFirstResource,ClickOk,ClickOk, PickFirstResource,ClickOk,PickFirstResource,ClickOk,PickFirstResource,ClickOk   },
        ()=>{ return RD0("worker") == 3; },"worker"); }
      },
      {"india_mughal_empire", ()=> { TestAction(
        "wonder ready get 1 vp", P0,
        ()=> { P0_DYN("india_mughal_empire"); P0_WIC("hanging_gardens",1); },
        new List<Action> { ClickArchitect, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD0("vp") == 1; }); }
      },
      {"emperor", ()=> { TestAction(
        "action: buy advisor place counter on this card", P0,
        ()=> { ProgressCard("elizabeth",0); P0_ADV("emperor");  },
        new List<Action> { ClickProg0, ClickOk, ClickPass },
        ()=>{ return P0.GetCard("emperor").NumDeployed == 1 && P0.Military == 1;}); }
      },
      {"japan_edo_period", ()=> { TestWar(
        "prod if defeated: no effect", P0,
        ()=> { P0_DYN("japan_edo_period"); P1_WAR(); P0.Books = 10; },
        new List<Action> {  },
        ()=>{ return RD0("book") == 0 && RD0("vp") == 0 && G.Stats.IsWar;}); }
      },
      {"japan_heian_period", ()=> { TestAction(
        "P0 buys ga, 4 books", P0,
        ()=> { P0_DYN("japan_heian_period"); ProgressCard("silk", 0);  },
        new List<Action> { ClickProg0, ClickOk, PickFirstResource, ClickOk, ClickPass },
        ()=>{ return RD0("book") == 4; }); }
      },
      {"korea_dyn1", ()=> { TestAction(
        "P0 buys ga, may hire 2 architects for free", P0,
        ()=> { P0_DYN("korea_dyn1"); ProgressCard("silk", 0); P0_WIC("colosseum",0);  },
        new List<Action> { ClickProg0, ClickOk, ClickOk, ClickWonder0, ClickOk, PickFirstResource, ClickOk, ClickPass },
        ()=>{ return !P0.HasWIC && RD0("coal") >= 0; }); }
      },
      {"korea_joseon_kingdom", ()=> { TestAction(
        "action: place up to 2 resources on dyn card: place 2 gold there!!!", P0,
        ()=> { P0_DYN("korea_joseon_kingdom"); },
        new List<Action> { ClickDynasty, ClickOk, PickResChoice, ClickOk, ClickPass },
        ()=>{ return RD0("gold") == 2; },"gold", 2); }
      },
      {"korea_koryo_kingdom", ()=> { TestWar(
        "P0 is above war because of dyn", P0,
        ()=> { P0_DYN("korea_koryo_kingdom"); P1_WAR("first_crusade",12); P0_MIL(6); },
        new List<Action> {},
        ()=>{ return RD0("vp") == 0 && RD0("book") == 0; }); }
      },
      {"mali_dyn1", ()=> { TestAction(
        "golden age bonus", P0,
        ()=> { P0_DYN("mali_dyn1"); ProgressCard("silk",0);  },
        new List<Action> {ClickProg0, ClickOk, PickFirstResource, ClickOk, ClickPass },
        ()=>{ return RD0("coal") == 3; }); }
      },
      {"mali_dyn1 growth", ()=> { TestGrowth(
        "golden age bonus", P0,
        ()=> { P0_DYN("mali_dyn1");  },
        new List<Action> { PickResource, ClickOk, PickFirstResource, ClickOk },
        ()=>{ return RD0("gold") == 5; },"gold"); }
      },
      {"mali_mali_empire", ()=> { TestAction(
        "action: remove adv and pay 2 gold for 3 book+1vp", P0,
        ()=> { P0_DYN("mali_mali_empire"); P0_ADV("archimedes");  },
        new List<Action> { ClickDynasty, ClickOk, ClickPass },
        ()=>{ return RD0("book") == 3 && RD0("gold") == -2 && RD0("vp") == 1 && !P0.HasRemovableAdvisor;}); }
      },
      {"mali_songhai_empire", ()=> { TestFamine(
        "famine = 1. other if least mil pay 3 more wheat for famine: only P1 should pay!", P0,
        ()=> { P0_DYN("mali_songhai_empire"); P0.Res.set("wheat",5);P1.Res.set("wheat",5);  },
        new List<Action> {  },
        ()=>{ return RD0("wheat") == -1 && RD1("wheat") == -4; }, "aryan_migration"); }
      },
      {"persia_achaimenid_empire", ()=> { TestAction(
        "action: place new worker for free", P0,
        ()=> { P0_DYN("persia_achaimenid_empire"); P0_BM0("hoplite"); P0.GrowthResourcePicked = new Res("worker");  },
        new List<Action> { ClickOk, ClickBM0, ClickOk, ClickPass },
        ()=>{ return RD0("coal") == 0 && P0.Military == 3;}); }
      },
      {"persia_sassanid_empire", ()=> { TestAction(
        "when gold does not hurt stability", P0,
        ()=> { P0_DYN("persia_sassanid_empire");   },
        new List<Action> { ClickTurmoil, ClickOk, PickChoice2, ClickOk, ClickPass },
        ()=>{ return P0.Stability == 0 && RD0("gold") == 2; }); }
      },
      {"persia_sassanid_empire dynasty", ()=> { TestAction(
        "when dynasty, do lose 2 stability", P0,
        ()=> { P0.Reset(true,"america"); P0_DYN("persia_sassanid_empire");   },
        new List<Action> { ClickTurmoil, ClickOk, PickChoice1, ClickOk, ClickPass },
        ()=>{ return P0.Stability == -2 && RD0("gold") == 0; }); }
      },
      {"poland_dyn1", ()=> { TestProduction(
        "gets 3 book if war and not defeated", P0,
        ()=> { P0_DYN("poland_dyn1"); P1_WAR("first_crusade",0);  },
        new List<Action> {  },
        ()=>{ return RD0("book") == 3; }); }
      },
      {"poland_jagellonian_dynasty", ()=> { TestAction(
        "pay 1 gold to first/second player and take action before all others", P0,
        ()=> { P0.Reset(true,"poland"); P0_DYN("poland_jagellonian_dynasty"); P1.HasPassed = false;  },
        new List<Action> { ClickOk, ClickTurmoil, ClickOk, PickChoice2, ClickOk, ClickPass, ClickPass },
        ()=>{ return P0.Stability == -2 && RD0("gold") == 1 && RD1("gold") == 1; },false); }
      },
      {"poland_jagellonian_dynasty 2", ()=> { TestAction(
        "pay 1 gold to first/second player and take action before all others", P0,
        ()=> { P1.Reset(true,"poland"); P1_DYN("poland_jagellonian_dynasty");  P1.HasPassed = false;   },
        new List<Action> { ClickOk, ClickTurmoil, ClickOk, PickChoice2, ClickOk, ClickPass, ClickPass },
        ()=>{ return P1.Stability == -2 && RD0("gold") == 1 && RD1("gold") == 1; },false); }
      },
      {"poland_polish-lithuanian_commonwealth", ()=> { TestAction(
        "poland_polish-lithuanian_commonwealth: Felix buys advisor, places it on wonder space, Amanda performs special action and buys it", P0,
        ()=> { P0.Reset(true,"poland"); P1.Reset(true, "egypt"); P0_DYN("poland_polish-lithuanian_commonwealth",0); P1.HasPassed = false; ProgressCard("archimedes",0); },
        new List<Action> {ClickProg0, ClickOk, ClickWonder0, ClickOk, ClickSpecialOption0, ClickOk, ClickPass, ClickPass },
        ()=>{ return RD0("gold") == 0 && RD1("gold") == -3 && P1.Advisors.FirstOrDefault() != null; },false); }
      },
      {"portugal_dyn1", ()=> { TestAction(
        "pay 2 for cards of price 3", P0,
        ()=> { P0_DYN("portugal_dyn1"); ProgressCard("forum",0);},
        new List<Action> {ClickProg0, ClickBM0, ClickOk, ClickPass},
        ()=>{ return RD0("gold") == -2; },"coal"); }
      },
      {"portugal_kingdom_of_leon", ()=> { TestAction(
        "pay war, get battle effect", P0,
        ()=> { P0_DYN("portugal_kingdom_of_leon"); ProgressCard("first_crusade",0); P0_MIL(6); },
        new List<Action> {ClickProg0, ClickOk, PickResource, ClickOk, ClickPass},
        ()=>{ return RD0("wheat") == 3; },"wheat"); }
      },
      {"portugal_portugese_empire", ()=> { TestAction(
        "portugal_portugese_empire: can place colonies on wonder fields", P0,
        ()=> { P0_DYN("portugal_portugese_empire"); P0_MIL(6); ProgressCard("nubia",0); },
        new List<Action> {ClickProg0, ClickOk, ClickWonder0, ClickOk, ClickPass },
        ()=>{ return RD0("gold") == -3 && P0.HasColony; }); }
      },
      {"persia", ()=> { TestAction(
        "persia check out start board", P0,
        ()=> { P0.Reset(true,"persia"); P0_MIL(6); ProgressCard("nubia",0); },
        new List<Action> { ClickProg0, ClickOk, ClickBM0, ClickOk, ClickPass },
        ()=>{ return RD0("gold") == -3 && P0.HasColony; }); }
      },
      {"rome_dyn1", ()=> { TestAction(
        "rome check out start board", P0,
        ()=> { P0.Reset(true,"rome"); },
        new List<Action> { ClickPass },
        ()=>{ return P0.Military == 2; }); }
      },
      {"rome_roman_empire", ()=> { TestProduction(
        "vp for most stab and mil, 2 book for most stab only", P0,
        ()=> { P0_DYN("rome_roman_empire"); P0_MIL(6); P0_ADV("buddha"); G.PassOrder = new List<Player> { P0, P1 }; },
        new List<Action> {  },
        ()=>{ return RD0("vp") == 1; }); }
      },
      {"rome_roman_republic", ()=> { TestAction(
        "discard architect, gain 1 stability", P0,
        ()=> { P0_DYN("rome_roman_republic"); P0_WIC("colosseum",1); },
        new List<Action> { ClickDynasty, ClickOk, ClickPass },
        ()=>{ return P0.WIC.NumDeployed == 0 && P0.Stability == 1; }); }
      },
      {"constantinople", ()=> { TestAction(
        "buy ga vp costs 1 less, so get vp for free", P0,
        ()=> { P0_COL0("constantinople"); ProgressCard("silk", 0); G.Stats.Age = 1; },
        new List<Action> { ClickProg0, ClickOk, PickResource, ClickOk, ClickPass },
        ()=>{ return RD0("vp") == 1; }, "vp", 1); }
      },
      {"venice_dyn1", ()=> { TestProduction(
        "P0 has passed last, he should get 2 book", P0,
        ()=> { P0_DYN("venice_dyn1"); G.PassOrder = new List<Player> { P1, P0 }; },
        new List<Action> {  },
        ()=>{ return RD0("book") == 2; }); }
      },
      {"venice_domains_of_the_sea", ()=> { TestAction(
        "buy colony, option: discard it for vp and 2 gold", P0,
        ()=> { ProgressCard("nubia",0); P0_DYN("venice_domains_of_the_sea"); P0_MIL(6); },
        new List<Action> { ClickProg0, ClickColony0, ClickOk, ClickOk, ClickPass },
        ()=>{ return !P0.HasColony && RD0("gold") == -1 && RD0("vp") == 1;}); }
      },
      {"venice_pactum_warmundi", ()=> { TestAction(
        "others buy war or battle: get 1 gold", P0,
        ()=> { P1_DYN("venice_pactum_warmundi"); ProgressCard("first_crusade",0); P0_MIL(3);  },
        new List<Action> {ClickProg0, ClickOk, ClickPass },
        ()=>{ return RD1("gold") == 1; }); }
      },
      {"old_uppsala", ()=> { TestAction(
        "click wonder to trade 1 wheat for 3 military until end of round", P0,
        ()=> { P0.Reset(true, "vikings"); P0_WOND0("old_uppsala"); },
        new List<Action> { ClickWonder0, ClickOk, ClickPass },
        ()=>{ return RD0("wheat") == -1 && P0.Military == 3; }); }
      },
      {"vikings_dyn1", ()=> { TestProduction(
        "P0 picks wheat, P1 defaults in both wheat and book", P0,
        ()=> { P0.Reset(true, "vikings"); P1.Reset(true,"japan"); P0_DYN("vikings_dyn1"); P1.Res.set("wheat",0); },
        new List<Action> { PickFirstResource, ClickOk, PickResChoice, ClickOk },
        ()=>{ return RD1("vp") == -2 && RD1("gold") == -1; },"gold",1); }
      },
      {"vikings_normans", ()=> { TestAction(
        "raid plus 3", P0,
        ()=> { P0_DYN("vikings_normans"); ProgressCard("milvian_bridge",0); P0_MIL(3);  },
        new List<Action> {ClickProg0, ClickOk, PickResource, ClickOk, ClickPass },
        ()=>{ return RD0("wheat") == 6; },"wheat"); }
      },
      {"vikings_varangians", ()=> { TestAction(
        "4 books when buy advisor", P0,
        ()=> { P0_DYN("vikings_varangians"); ProgressCard("archimedes",0); P0_MIL(3);  },
        new List<Action> {ClickProg0, ClickOk, ClickPass },
        ()=>{ return RD0("book") == 4; }); }
      },

          //******************************************************************HIER!!!!!!!!!!!!!!
      
      // automatic tests: Events: ACHTUNG!!! name des events vor [SPC] wird als name of event card benuetzt!!!!!
      {"american_revolution", ()=> { TestEventResolution(
        "P0 is least mil and most stab, P0 loses 1 colony, P1 loses advisor", P0,
        ()=> {P0.Reset(true,"america"); P1.Reset(true,"egypt"); P1_MIL(6); P0_ADV("buddha"); P1_ADV("sun_tzu"); P0_COL0("armenia"); },
        new List<Action> {  },
        ()=>{ return !P0.HasColony && !P1.HasRemovableAdvisor; }); }
      },
      {"anarchism", ()=> { TestEventResolution(
        "P0 is least mil and most stab", P0,
        ()=> { P1.Reset(true,"egypt"); P1_MIL(6); P0_ADV("buddha"); P1_ADV("sun_tzu"); P0_WIC("colosseum",0); },
        new List<Action> { ClickOk, ClickWonder1, ClickOk },
        ()=>{ return RD0("vp") == -1 && !P0.HasRemovableAdvisor && !P0.HasWIC; }); }
      },
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
      {"california_gold_rush", ()=> { TestEventResolution(
        "P0 is least mil and most stab", P0,
        ()=> { P1.Reset(true,"america"); P1_MIL(6); P0_ADV("buddha"); P0_COL0("armenia"); P1.Res.set("wheat",10); },
        new List<Action> {  },
        ()=>{ return RD1("gold") == 8 && RD1("wheat") == -8; }); }
      },
      {"entente_cordiale", ()=> { TestEventResolution(
        "P0 is least mil and most stab", P0,
        ()=> { P1.Reset(true,"america"); P1_MIL(6); P0_ADV("buddha"); P1.Res.set("coal",10); P0_COL0("armenia"); },
        new List<Action> { ClickOk },
        ()=>{ Checker.CalcStabAndMil(P1); return RD1("coal") == -6 && P1.Military == 12; }); }
      },
      {"eruption_of_krakatoa", ()=> { TestEventResolution( 
        "P0 is least mil and most stab", P0,
        ()=> { P1.Reset(true,"egypt"); P1_MIL(6); P0_ADV("buddha"); P1.Res.set("wheat",10); P1_ADV("sun_tzu"); P0_COL0("armenia"); },
        new List<Action> {  },
        ()=>{ return RD0("vp") == -1 && RD0("book") == 10 ; }); }
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
      {"march_to_moscow", ()=> { TestEventResolution(
        "P0 is least mil and most stab", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0_COL0("armenia"); P1.Res.set("wheat",5); P1.Books = 20; },
        new List<Action> {  },
        ()=>{ return !P0.HasColony && RD0("vp") == 0 && RD1("book") == -10 && G.Players[0] == P0; }); }
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
      {"romanticism", ()=> { TestEventResolution(
        "P0 is least mil and most stab, P1 is most wheat takes worker", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P1_BM1("coal_mine",2); G.PassOrder = new List<Player> {P0,P1 }; },
        new List<Action> {  },
        ()=>{ return RD0("book") == 5 && RD1("vp") == 1 ;  }); }
      },
      {"scramble_for_africa", ()=> { TestEventResolution(
        "P0 is least mil and most stab, P1 is least stab", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0_COL0("congo"); P1.Books = 10; P1.Res.set("wheat",5); },
        new List<Action> {  },
        ()=>{ return RD0("vp") == 1 && RD1("book") == -8;  }); }
      },
      {"sick_man_of_europe", ()=> { TestEventResolution(
        "P0 is least mil and most stab, P1 is most wheat takes worker", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.Res.set("gold",9); },
        new List<Action> {  },
        ()=>{ return RD0("gold") == -8 && RD0("vp") == 1;  }); }
      },
      {"sokoto_caliphate", ()=> { TestEventResolution(
        "P0 is least mil and most stab, all may trade 4 wheat for 8 gold, but P0 does not have 4 wheat, so only P1 trades!", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P1.Res.set("wheat",5); },
        new List<Action> { ClickOk }, //P0 is not asked because lacks wheat
        ()=>{ return RD1("gold") == 1 && RD1("wheat") == -4 ;  }); }
      },
      {"tonghak_movement", ()=> { TestEventResolution(
        "P0 is least mil and most stab, ", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P1.Res.set("wheat",5); P0.Books = 10; },
        new List<Action> {  },
        ()=>{ return RD0("book") == -8 && RD0("coal") == 2 && RD1("gold") == -5 ;  }); }
      },
      {"weltpolitik", ()=> { TestEventResolution(   //*******************************************
        "P0 is least mil and most stab, P1 is most wheat takes worker", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P1.Res.set("wheat",5); P0_COL0("congo"); P0_COL1("algeria"); },
        new List<Action> {  },
        ()=>{ return RD0("vp") == 2 && RD1("worker") == -1 ;  }); }
      },
      {"sinking_of_the_vasa", ()=> { TestEventResolution(
        "P0 is least mil and least stab: choses to pay gold and wheat", P0,
        ()=> { P1.Reset(true,"arabia"); P1_MIL(6); P1_ADV("buddha"); P0.Res.set("wheat",5); P0.Books=11; },
        new List<Action> { PickChoice0, ClickOk },
        ()=>{ return RD0("gold") == -3 && RD0("wheat") == -5 && RD1("wheat") == 0 && RD1("gold") == 0;  }); }
      },
      {"spice_trade", ()=> { TestEventResolution(
        "P0 is least mil and most stab, P1 is most wheat", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P1.Res.set("wheat",5); },
        new List<Action> {  },
        ()=>{ return RD1("gold") == 4 && G.Players[0] == P0 && RD1("coal") == -3 ;  }); }
      },
      {"tulip_mania", ()=> { TestEventResolution(
        "P0 is least mil and most stab, P1 passed last:lose 1 vp", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P1.Res.set("wheat",5); G.PassOrder = new List<Player> {P0,P1 };  },
        new List<Action> { },
        ()=>{ return RD1("vp") == -1 && G.Players[0] == P0 && RD0("gold") == 3 ;  }); }
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
        ()=> {P1.Reset(true, "mali"); P1_WAR("first_crusade",6); P0_MIL(9); P1_ADV("buddha");  },
        new List<Action> {  },
        ()=>{ return RD1("vp") == 0 && RD0("vp") == 1 && G.Players[0] == P1; }); }
      },
      {"rigveda", ()=> { TestEventResolution(
        "P0 bought 1 battle and 1 war, no one has most wheat", P0,
        ()=> { P0.Res.set("wheat",10);P1.Res.set("wheat",10); P0.CardsBoughtThisRound.Add(Card.MakeCard("first_crusade"));P0.CardsBoughtThisRound.Add(Card.MakeCard("milvian_bridge")); },
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
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.Res.set("wheat", 8); P0_WIC("colosseum",1); P1.Books = 5; },
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
        ()=> { P1.Reset(true, "mali"); P1_MIL(6); P0_ADV("buddha"); P1_ADV("anna_komnene"); },
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
        ()=>{ return RD0("book") == -5 && RD0("gold") == 4; }); }
      },
      {"stupor_mundi", ()=> { TestEventResolution(
        "P0 is most stab, P1 is most mil", P0,
        ()=> { P1_MIL(6); P0_ADV("buddha"); P0.CardsBoughtThisRound.Add(Card.MakeCard("silk")); },
        new List<Action> {  },
        ()=>{ return RD0("book") == 4 && RD1("gold") == -3; }); }
      },
      {"absolute_monarchy", ()=> { TestEventResolution(
        "P0 is most stab, P1 is most mil", P0,
        ()=> { P1.Reset(true,"egypt"); P1_MIL(6); P0_ADV("buddha"); P0.Books = 5; },
        new List<Action> {  },
        ()=>{ return RD0("book") == -4 && RD1("gold") == -3; }); }
      },
      
      

      // automatic tests: Progress cards
      {"abraham_lincoln", ()=> { TestAction(
        "pick extra worker", P0,
        ()=> { P0.Reset(true,"america"); P1.Reset(true, "egypt"); P0_ADV("abraham_lincoln"); },
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
        new List<Action> { ClickAdvisor, ClickOk, ClickProg0, ClickProg7, ClickOk, ClickPass },
        ()=>{ return G.Progress.Fields[7].Card.BasicCost == 2; }); }
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
        ()=>{ return RD0("book") == 2; }, "book",false); }
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
        ()=> { P0.Reset(true,"america"); P0_COL0("algeria"); P0_WOND0("big_ben");   },
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
      {"colosseum", ()=> { TestAction(
        "when ready, minus 2 wheat", P0,
        ()=> { P0.Reset(true,"america"); P0_WIC("colosseum",1); },
        new List<Action> { ClickArchitect, ClickOk, ClickWonder1, ClickOk, ClickPass },
        ()=>{ return RD0("military") == 3 && RD0("wheat") == -2 && RD0("coal") == -2; }); }
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
        ()=>{ return true; },false); }
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
        ()=> { P0.Reset(true,"america"); P0_WOND0("grand_canyon"); P0_WOND1("cape_of_good_hope");   },
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
        ()=> { P1.Reset(true,"egypt"); P1_ADV("hannibal");P0_BM0("hoplite",1); ProgressCard("milvian_bridge",0);},
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
        ()=>{ return !P0.HasRemovableAdvisor && RD0("book") == 0;  }); }
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
        ()=> {P1.Reset(true, "mali");  P1_ADV("martin_luther"); P1_WAR("first_crusade",6); P0.Books=8; },
        new List<Action> {},
        ()=>{ return RD0("vp") == -2 && RD0("book") == -8 && RD0("wheat") == -2; },"gold",2); }
      },
      {"mit", ()=> { TestAction(
        "deploy 1 worker when buy building", P0,
        ()=> { P0_WOND0("mit"); Progress0("ziggurat"); P0_BM0("forum",4);P0_BM1("aqueduct",1);  P0.Res.set("worker",0); },
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
        ()=> { P0.Reset(true,"america"); P0_WOND0("potala_palace"); P0_ADV("archimedes"); P0_BM0("hoplite",3);  },
        new List<Action> {  },
        ()=>{ return P0.Score == 11.1; }); }
      },
      {"qin_shi_huang", ()=> { TestAction(
        "least stability remove, P0 loses stability", P0,
        ()=> {P1.Reset(true, "mali");  P0_ADV("qin_shi_huang"); P0_Deploy(4,1); P1_ADV("saint_augustine");Progress0("forge");  },
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
        ()=> { P1.Reset(true,"egypt"); P0_ADV("saint_augustine");  },
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
        ()=>{ return P0.Stability == 2; },false); }
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
        ()=> { P0.Reset(true,"america"); P0_WOND0("statue_of_liberty"); P0_ADV("buddha"); P0_BM0("hoplite",3);  },
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
        ()=> { P0.Reset(true,"america"); P0_WOND0("titusville");  },
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
        ()=> { P1.Reset(true, "mali"); P0_WIC("titanic",2); P0_ADV("archimedes"); P1_ADV("archimedes"); },
        new List<Action> { ClickArchitect, ClickOk, ClickWonder1, ClickOk, PickChoice1, ClickOk, PickChoice0, ClickOk, ClickPass },
        ()=>{ return !P0.HasRemovableAdvisor && RD0("gold") == 0;  }); }
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
        ()=>{ return true; },false); }
      },

    };


  }
}

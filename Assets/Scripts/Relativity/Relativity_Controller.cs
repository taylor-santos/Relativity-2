using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relativity_Controller : MonoBehaviour {

	public GameObject Observer;
	public Vector3 Velocity; //Velocity relative to coordinate frame
	public float ProperTimeOffset;
	public List<Vector3> ProperAccelerations;
	public List<float> AccelerationDurations;
	public List<Vector3> AccelerationOffsets;
	public bool DrawMinkowski;
	public bool DrawWorldLine = true;
	public uint AccelerationCurveDetail = 10;
	public Vector3 Current_Velocity;
	public float Proper_Time;
	public Vector4 Current_Event;
	public AnimationCurve ColorSpectrum = AnimationCurve.Linear(400, 0, 700, 0);
	public int system;
	public bool method;

	private Relativity_Observer observerScript;
	private Vector3 observerVelocity; //Observer's velocity relative to coordinate frame
	private List<Material> mats;
	private MeshFilter[] MFs;

	private static double[,] rgb_colour_match = new double[69,3]{ /*390 through 730, stepsize 5*/
		{ 0.0018397, -0.00045393,  0.0121520}, { 0.0046153, -0.00104640,  0.0311100}, { 0.0096264, -0.00216890,  0.0623710},
		{ 0.0189790, -0.00443040,  0.1316100}, { 0.0308030, -0.00720480,  0.2275000}, { 0.0424590, -0.01257900,  0.3589700},
		{ 0.0516620, -0.01665100,  0.5239600}, { 0.0528370, -0.02124000,  0.6858600}, { 0.0442870, -0.01993600,  0.7960400},
		{ 0.0322200, -0.01609700,  0.8945900}, { 0.0147630, -0.00734570,  0.9639500}, {-0.0023392,  0.00136900,  0.9981400},
		{-0.0291300,  0.01961000,  0.9187500}, {-0.0606770,  0.04346400,  0.8248700}, {-0.0962240,  0.07095400,  0.7855400},
		{-0.1375900,  0.11022000,  0.6672300}, {-0.1748600,  0.15088000,  0.6109800}, {-0.2126000,  0.19794000,  0.4882900},
		{-0.2378000,  0.24042000,  0.3619500}, {-0.2567400,  0.27993000,  0.2663400}, {-0.2772700,  0.33353000,  0.1959300},
		{-0.2912500,  0.40521000,  0.1473000}, {-0.2950000,  0.49060000,  0.1074900}, {-0.2970600,  0.59673000,  0.0767140},
		{-0.2675900,  0.70184000,  0.0502480}, {-0.2172500,  0.80852000,  0.0287810}, {-0.1476800,  0.91076000,  0.0133090},
		{-0.0351840,  0.98482000,  0.0021170}, { 0.1061400,  1.03390000, -0.0041574}, { 0.2598100,  1.05380000, -0.0083032},
		{ 0.4197600,  1.05120000, -0.0121910}, { 0.5925900,  1.04980000, -0.0140390}, { 0.7900400,  1.03680000, -0.0146810},
		{ 1.0078000,  0.99826000, -0.0149470}, { 1.2283000,  0.93783000, -0.0146130}, { 1.4727000,  0.88039000, -0.0137820},
		{ 1.7476000,  0.82835000, -0.0126500}, { 2.0214000,  0.74686000, -0.0113560}, { 2.2724000,  0.64930000, -0.0099317},
		{ 2.4896000,  0.56317000, -0.0084148}, { 2.6725000,  0.47675000, -0.0070210}, { 2.8093000,  0.38484000, -0.0057437},
		{ 2.8717000,  0.30069000, -0.0042743}, { 2.8525000,  0.22853000, -0.0029132}, { 2.7601000,  0.16575000, -0.0022693},
		{ 2.5989000,  0.11373000, -0.0019966}, { 2.3743000,  0.07468200, -0.0015069}, { 2.1054000,  0.04650400, -0.0009382},
		{ 1.8145000,  0.02633300, -0.0005532}, { 1.5247000,  0.01272400, -0.0003167}, { 1.2543000,  0.00450330, -0.0001432},
		{ 1.0076000,  0.00009661, -0.0000041}, { 0.7864200, -0.00196450,  0.0001108}, { 0.5965900, -0.00263270,  0.0001918},
		{ 0.4432000, -0.00262620,  0.0002266}, { 0.3241000, -0.00230270,  0.0002152}, { 0.2345500, -0.00187000,  0.0001636},
		{ 0.1688400, -0.00144240,  0.0000972}, { 0.1208600, -0.00107550,  0.0000510}, { 0.0858110, -0.00079004,  0.0000353},
		{ 0.0602600, -0.00056765,  0.0000312}, { 0.0414800, -0.00039274,  0.0000245}, { 0.0281140, -0.00026231,  0.0000165},
		{ 0.0191170, -0.00017512,  0.0000111}, { 0.0133050, -0.00012140,  0.0000087}, { 0.0094092, -0.00008576,  0.0000074},
		{ 0.0065177, -0.00005768,  0.0000061}, { 0.0045377, -0.00003900,  0.0000050}, { 0.0031742, -0.00002651,  0.0000041}
	};

	private static double[,] new_rgb_colour_match = new double[341,3]{
		{0.0001587409684,-0.00008558141781,0.003911662773},
		{0.0002066402381,-0.0001079216124,0.005132158212},
		{0.0002545395077,-0.000130261807,0.00635265365},
		{0.0003024387774,-0.0001526020015,0.007573149089},
		{0.0003503380471,-0.0001749421961,0.008793644527},
		{0.0003982373167,-0.0001972823907,0.01001413997},
		{0.0004847152063,-0.0002396083623,0.01202668918},
		{0.0005711930958,-0.0002819343339,0.01403923838},
		{0.0006576709853,-0.0003242603055,0.01605178759},
		{0.0007441488749,-0.0003665862772,0.0180643368},
		{0.0008306267644,-0.0004089122488,0.02007688601},
		{0.0009920270776,-0.0004941863555,0.02453441759},
		{0.001153427391,-0.0005794604623,0.02899194917},
		{0.001314827704,-0.000664734569,0.03344948075},
		{0.001476228017,-0.0007500086758,0.03790701233},
		{0.001637628331,-0.0008352827825,0.04236454391},
		{0.001841678253,-0.0009398967475,0.0485378379},
		{0.002045728175,-0.001044510712,0.0547111319},
		{0.002249778098,-0.001149124677,0.0608844259},
		{0.00245382802,-0.001253738642,0.0670577199},
		{0.002657877942,-0.001358352607,0.07323101389},
		{0.002859028644,-0.001560996916,0.08169490963},
		{0.003060179345,-0.001763641225,0.09015880536},
		{0.003261330047,-0.001966285534,0.09862270109},
		{0.003462480748,-0.002168929843,0.1070865968},
		{0.00366363145,-0.002371574152,0.1155504926},
		{0.003822450076,-0.002525116563,0.1261723695},
		{0.003981268702,-0.002678658974,0.1367942464},
		{0.004140087328,-0.002832201385,0.1474161233},
		{0.004298905954,-0.002985743796,0.1580380002},
		{0.004457724581,-0.003139286207,0.1686598771},
		{0.004478001869,-0.003312323075,0.1790828232},
		{0.004498279157,-0.003485359942,0.1895057692},
		{0.004518556446,-0.00365839681,0.1999287153},
		{0.004538833734,-0.003831433677,0.2103516614},
		{0.004559111023,-0.004004470545,0.2207746074},
		{0.004411561392,-0.003955300774,0.2278678761},
		{0.004264011761,-0.003906131004,0.2349611448},
		{0.00411646213,-0.003856961234,0.2420544134},
		{0.003968912499,-0.003807791464,0.2491476821},
		{0.003821362868,-0.003758621694,0.2562409508},
		{0.00361311943,-0.003613864986,0.2625854927},
		{0.003404875992,-0.003469108277,0.2689300346},
		{0.003196632554,-0.003324351569,0.2752745765},
		{0.002988389116,-0.003179594861,0.2816191184},
		{0.002780145677,-0.003034838152,0.2879636603},
		{0.002478885571,-0.002704853943,0.2924289817},
		{0.002177625465,-0.002374869733,0.2968943032},
		{0.001876365359,-0.002044885523,0.3013596246},
		{0.001575105253,-0.001714901313,0.305824946},
		{0.001273845147,-0.001384917104,0.3102902674},
		{0.0009787079191,-0.001056312966,0.3124913825},
		{0.0006835706913,-0.0007277088276,0.3146924975},
		{0.0003884334635,-0.0003991046896,0.3168936126},
		{0.00009329623564,-0.00007050055157,0.3190947276},
		{-0.0002018409922,0.0002581035864,0.3212958427},
		{-0.0006641769749,0.0009459147654,0.3161848007},
		{-0.001126512958,0.001633725944,0.3110737588},
		{-0.00158884894,0.002321537123,0.3059627169},
		{-0.002051184923,0.003009348302,0.300851675},
		{-0.002513520906,0.003697159481,0.295740633},
		{-0.003057935901,0.004596619372,0.2896967407},
		{-0.003602350896,0.005496079262,0.2836528483},
		{-0.004146765891,0.006395539153,0.2776089559},
		{-0.004691180885,0.007294999043,0.2715650635},
		{-0.00523559588,0.008194458934,0.2655211711},
		{-0.005849039942,0.009231021036,0.2629891485},
		{-0.006462484005,0.01026758314,0.2604571258},
		{-0.007075928067,0.01130414524,0.2579251031},
		{-0.007689372129,0.01234070734,0.2553930805},
		{-0.008302816191,0.01337726945,0.2528610578},
		{-0.009016680288,0.01485786775,0.2452443886},
		{-0.009730544385,0.01633846606,0.2376277194},
		{-0.01044440848,0.01781906437,0.2300110501},
		{-0.01115827258,0.01929966268,0.2223943809},
		{-0.01187213668,0.02078026099,0.2147777117},
		{-0.01251531501,0.02231342269,0.2111563978},
		{-0.01315849334,0.02384658439,0.2075350839},
		{-0.01380167168,0.02537974608,0.20391377},
		{-0.01444485001,0.02691290778,0.2002924562},
		{-0.01508802834,0.02844606948,0.1966711423},
		{-0.01573931759,0.0302205552,0.1887724934},
		{-0.01639060684,0.03199504093,0.1808738445},
		{-0.01704189609,0.03376952665,0.1729751957},
		{-0.01769318533,0.03554401237,0.1650765468},
		{-0.01834447458,0.0373184981,0.1571778979},
		{-0.0187793577,0.03892028632,0.149044266},
		{-0.01921424083,0.04052207453,0.1409106341},
		{-0.01964912395,0.04212386275,0.1327770022},
		{-0.02008400707,0.04372565097,0.1246433703},
		{-0.0205188902,0.04532743919,0.1165097384},
		{-0.02084574283,0.04681723797,0.1103544705},
		{-0.02117259546,0.04830703676,0.1041992026},
		{-0.02149944809,0.04979683555,0.09804393466},
		{-0.02182630073,0.05128663434,0.09188866676},
		{-0.02215315336,0.05277643312,0.08573339886},
		{-0.02250744505,0.05479752183,0.08120047957},
		{-0.02286173673,0.05681861055,0.07666756029},
		{-0.02321602842,0.05883969926,0.072134641},
		{-0.02357032011,0.06086078797,0.06760172171},
		{-0.02392461179,0.06288187668,0.06306880243},
		{-0.02416586838,0.06558470577,0.05993805587},
		{-0.02440712497,0.06828753485,0.05680730932},
		{-0.02464838156,0.07099036393,0.05367656276},
		{-0.02488963815,0.07369319301,0.05054581621},
		{-0.02513089474,0.0763960221,0.04741506966},
		{-0.02519560949,0.07961581249,0.04485214512},
		{-0.02526032424,0.08283560289,0.04228922058},
		{-0.02532503899,0.08605539328,0.03972629604},
		{-0.02538975374,0.08927518367,0.0371633715},
		{-0.02545446849,0.09249497407,0.03460044696},
		{-0.02549001846,0.09649680513,0.03261912152},
		{-0.02552556843,0.1004986362,0.03063779609},
		{-0.0255611184,0.1045004673,0.02865647065},
		{-0.02559666837,0.1085022983,0.02667514522},
		{-0.02563221834,0.1125041294,0.02469381978},
		{-0.02512364669,0.1164674994,0.02298996746},
		{-0.02461507504,0.1204308695,0.02128611514},
		{-0.02410650338,0.1243942395,0.01958226282},
		{-0.02359793173,0.1283576096,0.0178784105},
		{-0.02308936008,0.1323209796,0.01617455818},
		{-0.02222062927,0.1363435495,0.01479253605},
		{-0.02135189846,0.1403661193,0.01341051392},
		{-0.02048316765,0.1443886892,0.01202849178},
		{-0.01961443684,0.148411259,0.01064646965},
		{-0.01874570603,0.1524338288,0.00926444752},
		{-0.01754511798,0.1562889801,0.008268376974},
		{-0.01634452993,0.1601441315,0.007272306427},
		{-0.01514394189,0.1639992828,0.00627623588},
		{-0.01394335384,0.1678544341,0.005280165333},
		{-0.01274276579,0.1717095854,0.004284094786},
		{-0.0108013923,0.1745021568,0.003563565989},
		{-0.00886001882,0.1772947283,0.002843037191},
		{-0.006918645337,0.1800872997,0.002122508393},
		{-0.004977271854,0.1828798712,0.001401979595},
		{-0.003035898371,0.1856724426,0.0006814507974},
		{-0.0005970324013,0.1875230963,0.0002775117437},
		{0.001841833568,0.1893737499,-0.00012642731},
		{0.004280699538,0.1912244035,-0.0005303663637},
		{0.006719565508,0.1930750571,-0.0009343054174},
		{0.009158431477,0.1949257107,-0.001338244471},
		{0.01181035566,0.1956760776,-0.00160514657},
		{0.01446227985,0.1964264445,-0.001872048669},
		{0.01711420403,0.1971768114,-0.002138950768},
		{0.01976612822,0.1979271783,-0.002405852866},
		{0.0224180524,0.1986775452,-0.002672754965},
		{0.02517835222,0.1985795073,-0.002923047304},
		{0.02793865204,0.1984814694,-0.003173339644},
		{0.03069895187,0.1983834315,-0.003423631983},
		{0.03345925169,0.1982853936,-0.003673924322},
		{0.03621955151,0.1981873558,-0.003924216661},
		{0.03920212492,0.1981345661,-0.004043188893},
		{0.04218469834,0.1980817765,-0.004162161125},
		{0.04516727176,0.1980289869,-0.004281133356},
		{0.04814984517,0.1979761972,-0.004400105588},
		{0.05113241859,0.1979234076,-0.00451907782},
		{0.05453986591,0.1974332182,-0.004560409082},
		{0.05794731324,0.1969430288,-0.004601740345},
		{0.06135476056,0.1964528393,-0.004643071607},
		{0.06476220789,0.1959626499,-0.004684402869},
		{0.06816965521,0.1954724605,-0.004725734132},
		{0.07192759763,0.1940192374,-0.004742858923},
		{0.07568554004,0.1925660143,-0.004759983714},
		{0.07944348245,0.1911127911,-0.004777108505},
		{0.08320142486,0.189659568,-0.004794233296},
		{0.08695936728,0.1882063449,-0.004811358087},
		{0.0907645946,0.1859277182,-0.004789855529},
		{0.09456982193,0.1836490915,-0.004768352972},
		{0.09837504925,0.1813704648,-0.004746850415},
		{0.1021802766,0.1790918381,-0.004725347858},
		{0.1059855039,0.1768132114,-0.004703845301},
		{0.1102031799,0.1746473283,-0.004650346424},
		{0.1144208559,0.1724814452,-0.004596847547},
		{0.1186385319,0.1703155621,-0.00454334867},
		{0.1228562079,0.1681496789,-0.004489849793},
		{0.1270738839,0.1659837958,-0.004436350916},
		{0.1318179065,0.1640215298,-0.004363473987},
		{0.1365619292,0.1620592638,-0.004290597057},
		{0.1413059518,0.1600969979,-0.004217720127},
		{0.1460499744,0.1581347319,-0.004144843197},
		{0.1507939971,0.1561724659,-0.004071966267},
		{0.1555190367,0.1530997323,-0.003988659953},
		{0.1602440764,0.1500269988,-0.003905353639},
		{0.164969116,0.1469542652,-0.003822047326},
		{0.1696941556,0.1438815316,-0.003738741012},
		{0.1744191953,0.1408087981,-0.003655434698},
		{0.1787507693,0.137130115,-0.003563739812},
		{0.1830823432,0.1334514318,-0.003472044926},
		{0.1874139172,0.1297727487,-0.003380350039},
		{0.1917454911,0.1260940656,-0.003288655153},
		{0.1960770651,0.1224153825,-0.003196960267},
		{0.1998253434,0.119167689,-0.003099303893},
		{0.2035736218,0.1159199955,-0.00300164752},
		{0.2073219001,0.1126723021,-0.002903991146},
		{0.2110701785,0.1094246086,-0.002806334772},
		{0.2148184568,0.1061769151,-0.002708678399},
		{0.2179748109,0.1029182866,-0.002618947069},
		{0.221131165,0.09965965814,-0.00252921574},
		{0.2242875191,0.09640102966,-0.002439484411},
		{0.2274438732,0.09314240118,-0.002349753081},
		{0.2306002273,0.0898837727,-0.002260021752},
		{0.2329610214,0.08641813345,-0.002177790566},
		{0.2353218155,0.08295249421,-0.00209555938},
		{0.2376826096,0.07948685497,-0.002013328194},
		{0.2400434037,0.07602121572,-0.001931097008},
		{0.2424041978,0.07255557648,-0.001848865822},
		{0.2434810512,0.06938254261,-0.001754267447},
		{0.2445579046,0.06620950875,-0.001659669072},
		{0.2456347581,0.06303647489,-0.001565070696},
		{0.2467116115,0.05986344102,-0.001470472321},
		{0.247788465,0.05669040716,-0.001375873946},
		{0.2474571255,0.05396947877,-0.001288247807},
		{0.2471257859,0.05124855039,-0.001200621668},
		{0.2467944464,0.048527622,-0.001112995529},
		{0.2464631069,0.04580669362,-0.00102536939},
		{0.2461317674,0.04308576523,-0.0009377432513},
		{0.2445371959,0.04071852737,-0.000896289669},
		{0.2429426245,0.03835128951,-0.0008548360868},
		{0.241348053,0.03598405165,-0.0008133825045},
		{0.2397534816,0.03361681379,-0.0007719289222},
		{0.2381589101,0.03124957593,-0.0007304753399},
		{0.2353770387,0.02928806409,-0.0007129192103},
		{0.2325951673,0.02732655224,-0.0006953630806},
		{0.2298132959,0.0253650404,-0.0006778069509},
		{0.2270314245,0.02340352856,-0.0006602508213},
		{0.2242495531,0.02144201672,-0.0006426946916},
		{0.220373571,0.01996963851,-0.0006111683378},
		{0.2164975889,0.0184972603,-0.0005796419839},
		{0.2126216068,0.01702488209,-0.0005481156301},
		{0.2087456246,0.01555250388,-0.0005165892762},
		{0.2048696425,0.01408012567,-0.0004850629224},
		{0.2002291635,0.01301762123,-0.0004484519219},
		{0.1955886844,0.0119551168,-0.0004118409215},
		{0.1909482054,0.01089261236,-0.0003752299211},
		{0.1863077264,0.009830107929,-0.0003386189207},
		{0.1816672473,0.008767603494,-0.0003020079202},
		{0.1766471084,0.008007018038,-0.0002772181759},
		{0.1716269695,0.007246432583,-0.0002524284315},
		{0.1666068306,0.006485847128,-0.0002276386872},
		{0.1615866917,0.005725261672,-0.0002028489428},
		{0.1565665528,0.004964676217,-0.0001780591984},
		{0.1515653969,0.004451523301,-0.000162834873},
		{0.146564241,0.003938370386,-0.0001476105476},
		{0.1415630851,0.00342521747,-0.0001323862222},
		{0.1365619292,0.002912064554,-0.0001171618968},
		{0.1315607733,0.002398911639,-0.0001019375713},
		{0.1268944083,0.002088934699,-0.00009076847367},
		{0.1222280434,0.001778957759,-0.000079599376},
		{0.1175616784,0.001468980819,-0.00006843027833},
		{0.1128953135,0.00115900388,-0.00005726118066},
		{0.1082289486,0.0008490269399,-0.00004609208299},
		{0.1039715809,0.0006828644512,-0.00003713653193},
		{0.09971421315,0.0005167019625,-0.00002818098087},
		{0.09545684544,0.0003505394738,-0.00001922542982},
		{0.09119947772,0.0001843769851,-0.00001026987876},
		{0.08694211001,0.00001821449641,-0.000001314327705},
		{0.08312514775,-0.00005950356657,0.000006082365221},
		{0.07930818548,-0.0001372216295,0.00001347905815},
		{0.07549122322,-0.0002149396925,0.00002087575107},
		{0.07167426095,-0.0002926577555,0.000028272444},
		{0.06785729868,-0.0003703758185,0.00003566913692},
		{0.06458135173,-0.000395571555,0.00004087996617},
		{0.06130540478,-0.0004207672915,0.00004609079541},
		{0.05802945783,-0.000445963028,0.00005130162465},
		{0.05475351088,-0.0004711587646,0.0000565124539},
		{0.05147756393,-0.0004963545011,0.00006172328314},
		{0.04883047177,-0.0004961094064,0.00006396431311},
		{0.04618337962,-0.0004958643116,0.00006620534308},
		{0.04353628747,-0.0004956192169,0.00006844637306},
		{0.04088919532,-0.0004953741222,0.00007068740303},
		{0.03824210317,-0.0004951290275,0.000072928433},
		{0.0361867627,-0.0004829308522,0.00007219708855},
		{0.03413142223,-0.0004707326768,0.00007146574409},
		{0.03207608175,-0.0004585345015,0.00007073439964},
		{0.03002074128,-0.0004463363261,0.00007000305518},
		{0.02796540081,-0.0004341381508,0.00006927171073},
		{0.02642001257,-0.0004178223843,0.00006595040259},
		{0.02487462433,-0.0004015066178,0.00006262909445},
		{0.02332923609,-0.0003851908512,0.00005930778631},
		{0.02178384785,-0.0003688750847,0.00005598647817},
		{0.02023845961,-0.0003525593182,0.00005266517004},
		{0.01910448461,-0.0003364358567,0.00004838744876},
		{0.01797050961,-0.0003203123953,0.00004410972749},
		{0.01683653461,-0.0003041889339,0.00003983200621},
		{0.01570255961,-0.0002880654724,0.00003555428494},
		{0.01456858461,-0.000271942011,0.00003127656367},
		{0.01374058095,-0.0002581073571,0.00002830670002},
		{0.01291257729,-0.0002442727032,0.00002533683637},
		{0.01208457363,-0.0002304380493,0.00002236697272},
		{0.01125656997,-0.0002166033954,0.00001939710907},
		{0.01042856631,-0.0002027687416,0.00001642724542},
		{0.009823716369,-0.0001920049359,0.00001541250498},
		{0.009218866426,-0.0001812411302,0.00001439776455},
		{0.008614016482,-0.0001704773246,0.00001338302412},
		{0.008009166539,-0.0001597135189,0.00001236828369},
		{0.007404316596,-0.0001489497132,0.00001135354326},
		{0.006963376173,-0.0001405640804,0.00001109216487},
		{0.006522435749,-0.0001321784476,0.00001083078648},
		{0.006081495326,-0.0001237928148,0.00001056940809},
		{0.005640554903,-0.000115407182,0.00001030802971},
		{0.005199614479,-0.0001070215492,0.00001004665132},
		{0.004875523009,-0.000100426239,0.000009615119461},
		{0.004551431539,-0.00009383092879,0.000009183587605},
		{0.004227340069,-0.00008723561859,0.000008752055749},
		{0.003903248599,-0.00008064030839,0.000008320523893},
		{0.003579157129,-0.0000740449982,0.000007888992038},
		{0.003348496501,-0.00006912688997,0.000007374797657},
		{0.003117835874,-0.00006420878175,0.000006860603276},
		{0.002887175246,-0.00005929067353,0.000006346408896},
		{0.002656514618,-0.00005437256531,0.000005832214515},
		{0.002425853991,-0.00004945445709,0.000005318020134},
		{0.002270590361,-0.00004616679431,0.000004970567139},
		{0.002115326732,-0.00004287913154,0.000004623114144},
		{0.001960063103,-0.00003959146876,0.000004275661149},
		{0.001804799474,-0.00003630380598,0.000003928208154},
		{0.001649535845,-0.00003301614321,0.000003580755159},
		{0.00154923661,-0.00003099052967,0.000003424475346},
		{0.001448937376,-0.00002896491613,0.000003268195534},
		{0.001348638141,-0.0000269393026,0.000003111915722},
		{0.001248338907,-0.00002491368906,0.00000295563591},
		{0.001148039672,-0.00002288807552,0.000002799356098},
		{0.001080808812,-0.00002154420236,0.000002718148537},
		{0.001013577952,-0.00002020032919,0.000002636940976},
		{0.0009463470925,-0.00001885645603,0.000002555733414},
		{0.0008791162326,-0.00001751258286,0.000002474525853},
		{0.0008118853727,-0.00001616870969,0.000002393318292},
		{0.0007619859858,-0.00001510978741,0.000002307732965},
		{0.0007120865989,-0.00001405086513,0.000002222147637},
		{0.000662187212,-0.00001299194285,0.00000213656231},
		{0.0006122878252,-0.00001193302057,0.000002050976982},
		{0.0005623884383,-0.00001087409829,0.000001965391655},
		{0.0005282190501,-0.00001016996003,0.000001895991187},
		{0.0004940496619,-0.000009465821771,0.000001826590718},
		{0.0004598802736,-0.000008761683514,0.000001757190249},
		{0.0004257108854,-0.000008057545256,0.000001687789781},
		{0.0003915414972,-0.000007353406999,0.000001618389312},
		{0.000368011214,-0.000006882372667,0.000001560280905},
		{0.0003444809307,-0.000006411338335,0.000001502172498},
		{0.0003209506475,-0.000005940304003,0.000001444064091},
		{0.0002974203642,-0.000005469269671,0.000001385955684},
		{0.000273890081,-0.000004998235339,0.000001327847277}
	};

    private static double[,] cie_colour_match = new double[81,3]{
        {0.0014,0.0000,0.0065}, {0.0022,0.0001,0.0105}, {0.0042,0.0001,0.0201},
        {0.0076,0.0002,0.0362}, {0.0143,0.0004,0.0679}, {0.0232,0.0006,0.1102},
        {0.0435,0.0012,0.2074}, {0.0776,0.0022,0.3713}, {0.1344,0.0040,0.6456},
        {0.2148,0.0073,1.0391}, {0.2839,0.0116,1.3856}, {0.3285,0.0168,1.6230},
        {0.3483,0.0230,1.7471}, {0.3481,0.0298,1.7826}, {0.3362,0.0380,1.7721},
        {0.3187,0.0480,1.7441}, {0.2908,0.0600,1.6692}, {0.2511,0.0739,1.5281},
        {0.1954,0.0910,1.2876}, {0.1421,0.1126,1.0419}, {0.0956,0.1390,0.8130},
        {0.0580,0.1693,0.6162}, {0.0320,0.2080,0.4652}, {0.0147,0.2586,0.3533},
        {0.0049,0.3230,0.2720}, {0.0024,0.4073,0.2123}, {0.0093,0.5030,0.1582},
        {0.0291,0.6082,0.1117}, {0.0633,0.7100,0.0782}, {0.1096,0.7932,0.0573},
        {0.1655,0.8620,0.0422}, {0.2257,0.9149,0.0298}, {0.2904,0.9540,0.0203},
        {0.3597,0.9803,0.0134}, {0.4334,0.9950,0.0087}, {0.5121,1.0000,0.0057},
        {0.5945,0.9950,0.0039}, {0.6784,0.9786,0.0027}, {0.7621,0.9520,0.0021},
        {0.8425,0.9154,0.0018}, {0.9163,0.8700,0.0017}, {0.9786,0.8163,0.0014},
        {1.0263,0.7570,0.0011}, {1.0567,0.6949,0.0010}, {1.0622,0.6310,0.0008},
        {1.0456,0.5668,0.0006}, {1.0026,0.5030,0.0003}, {0.9384,0.4412,0.0002},
        {0.8544,0.3810,0.0002}, {0.7514,0.3210,0.0001}, {0.6424,0.2650,0.0000},
        {0.5419,0.2170,0.0000}, {0.4479,0.1750,0.0000}, {0.3608,0.1382,0.0000},
        {0.2835,0.1070,0.0000}, {0.2187,0.0816,0.0000}, {0.1649,0.0610,0.0000},
        {0.1212,0.0446,0.0000}, {0.0874,0.0320,0.0000}, {0.0636,0.0232,0.0000},
        {0.0468,0.0170,0.0000}, {0.0329,0.0119,0.0000}, {0.0227,0.0082,0.0000},
        {0.0158,0.0057,0.0000}, {0.0114,0.0041,0.0000}, {0.0081,0.0029,0.0000},
        {0.0058,0.0021,0.0000}, {0.0041,0.0015,0.0000}, {0.0029,0.0010,0.0000},
        {0.0020,0.0007,0.0000}, {0.0014,0.0005,0.0000}, {0.0010,0.0004,0.0000},
        {0.0007,0.0002,0.0000}, {0.0005,0.0002,0.0000}, {0.0003,0.0001,0.0000},
        {0.0002,0.0001,0.0000}, {0.0002,0.0001,0.0000}, {0.0001,0.0000,0.0000},
        {0.0001,0.0000,0.0000}, {0.0001,0.0000,0.0000}, {0.0000,0.0000,0.0000}
    };
    /* For NTSC television */
	private static float IlluminantCx = 0.3101f;
	private static float IlluminantCy = 0.3162f; 
	/* For EBU and SMPTE */       
	private static float IlluminantD65x = 0.3127f;
	private static float IlluminantD65y = 0.3291f;
	/* CIE equal-energy illuminant */      
	private static float IlluminantEx = 0.33333333f;
	private static float IlluminantEy = 0.33333333f;
	/* Rec. 709 */
	private static float GAMMA_REC709 = 0;     

    struct colourSystem {
		public string name;                     /* Colour system name */
		public double xRed, yRed,              /* Red x, y */
		           xGreen, yGreen,          /* Green x, y */
		           xBlue, yBlue,            /* Blue x, y */
		           xWhite, yWhite,          /* White point x, y */
		           gamma;                   /* Gamma correction for system */
		public colourSystem(string name_in, 
		    	double xRed_in, double yRed_in,
		    	double xGreen_in, double yGreen_in,
		    	double xBlue_in, double yBlue_in,
		    	double xWhite_in, double yWhite_in,
		    	double gamma_in){
		    	name   = name_in;
		    	xRed   = xRed_in;
		    	yRed   = yRed_in;
		    	xGreen = xGreen_in;
		    	yGreen = yGreen_in;
		    	xBlue  = xBlue_in;
		    	yBlue  = yBlue_in;
		    	xWhite = xWhite_in;
		    	yWhite = yWhite_in;
		    	gamma  = gamma_in;
		    }
	};
	
    /*               Name                  xRed    yRed    xGreen  yGreen  xBlue  yBlue    White point     Gamma*/
    private static colourSystem[] systems = new colourSystem[6]{
   		new colourSystem( "NTSC",               0.67,   0.33,   0.21,   0.71,   0.14,   0.08,   IlluminantCx, IlluminantCy,    GAMMA_REC709 ),
		new colourSystem( "EBU",   0.64,   0.33,   0.29,   0.60,   0.15,   0.06,   IlluminantD65x, IlluminantD65y,  GAMMA_REC709 ),
		new colourSystem( "SMPTE",              0.630,  0.340,  0.310,  0.595,  0.155,  0.070,  IlluminantD65x, IlluminantD65y,  GAMMA_REC709 ),
		new colourSystem( "HDTV",               0.670,  0.330,  0.210,  0.710,  0.150,  0.060,  IlluminantD65x, IlluminantD65y,  GAMMA_REC709 ),
		new colourSystem( "CIE",                0.7355, 0.2645, 0.2658, 0.7243, 0.1669, 0.0085, IlluminantEx, IlluminantEy,   GAMMA_REC709 ),
		new colourSystem( "CIE REC 709",        0.64,   0.33,   0.30,   0.60,   0.15,   0.06,   IlluminantD65x, IlluminantD65y, GAMMA_REC709 )
	};

	void Start () {
		observerScript = Observer.GetComponent<Relativity_Observer>();
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		mats = new List<Material>();
		foreach (Renderer R in renderers){
			Material[] childMaterials = R.materials;
			foreach (Material mat in childMaterials){
				if (mat.shader == Shader.Find("Relativity/Relativity_Shader"))
					mats.Add(mat);
			}
		}
		MFs = GetComponentsInChildren<MeshFilter>();
		if (mats.Count != 0){
			if (ProperAccelerations.Count > 0 && AccelerationOffsets.Count > 0){
				Vector4[] shader_ProperAccelerations = new Vector4[ProperAccelerations.Count];
				Vector4[] shader_AccelerationOffsets = new Vector4[ProperAccelerations.Count];
				for (int i=0; i<ProperAccelerations.Count; ++i){
					shader_ProperAccelerations[i] = (Vector4)ProperAccelerations[i];
					shader_AccelerationOffsets[i] = (Vector4)AccelerationOffsets[i];
				}
				foreach(Material mat in mats){
					mat.SetVectorArray("_accelerations", shader_ProperAccelerations);
					mat.SetFloatArray("_durations", AccelerationDurations.ToArray());
					mat.SetVectorArray("_accel_positions", shader_AccelerationOffsets);
				}
			}
			foreach(Material mat in mats){
				mat.SetVector("_Velocity", Velocity);
				mat.SetFloat("_Proper_Time_Offset", ProperTimeOffset);
			}
		}
	}
	
	void Update () {
		if (method){
			Vector3 xyz = spectrum_to_xyz();
			int index = 0;
			Color col = xyz_to_rgb(systems[system], xyz.x, xyz.y, xyz.z);
			foreach(Material mat in mats){
				mat.SetColor("_Color", col);
			}
		}else{
			Color col = spectrum_to_rgb();
			foreach(Material mat in mats){
				mat.SetColor("_Color", col);
			}
		}
		observerVelocity = observerScript.velocity;
		float observer_time = observerScript.CoordinateTime;
		
		for (int i=AccelerationOffsets.Count; i<ProperAccelerations.Count; ++i){
			if (AccelerationOffsets.Count == 0)
				AccelerationOffsets.Add(Vector4.zero);
			else
				AccelerationOffsets.Add(AccelerationOffsets[i-1]);
		}

		Current_Event = get_state(observer_time); //Object's current event in observer's frame
		foreach (MeshFilter MF in MFs){
			Bounds newBounds = MF.sharedMesh.bounds;
			newBounds.center = transform.InverseTransformPoint(get_spatial_component(Current_Event));
			newBounds.extents = new Vector3(20,20,20);
			MF.sharedMesh.bounds = newBounds;
		}
		if (DrawWorldLine)
			draw_path(observer_time);
		if (DrawMinkowski)
		{
			Matrix4x4 coordinate_observer_boost = lorentz_boost(observerVelocity);
			Color[] colors = new Color[3]{Color.blue, Color.red, Color.green};
			float[] alpha = new float[3]{0.15f, 1f, 0.1f};
			int sep = 15;
			int[] seps = new int[3]{0, 1, -1};
			for (int i=0; i<3; ++i){
				for (int j=0; j<2; ++j){
					for (int k=0; k<3; ++k){
						for (int m=0; m<3; ++m){
							Vector4 dir = new Vector4();
							dir[i] = 1;
							Vector4 pt = new Vector4();
							pt[(i+1)%3] = seps[k]*sep;
							pt[(i+2)%3] = seps[m]*sep;
							Debug.DrawRay(boost_to_minkowski(pt, coordinate_observer_boost), boost_to_minkowski(dir, coordinate_observer_boost)*100 * (j==0?1:-1), new Color(colors[i].r, colors[i].g, colors[i].b, k==0&&m==0?alpha[1]:alpha[0]));
						}
					}
				}
			}
		}
	}

	Color xyz_to_rgb(colourSystem cs, double xc, double yc, double zc){
	    double xr, yr, zr, xg, yg, zg, xb, yb, zb;
	    double xw, yw, zw;
	    double rx, ry, rz, gx, gy, gz, bx, by, bz;
	    double rw, gw, bw;

	    xr = cs.xRed;    yr = cs.yRed;    zr = 1 - (xr + yr);
	    xg = cs.xGreen;  yg = cs.yGreen;  zg = 1 - (xg + yg);
	    xb = cs.xBlue;   yb = cs.yBlue;   zb = 1 - (xb + yb);

	    xw = cs.xWhite;  yw = cs.yWhite;  zw = 1 - (xw + yw);

	    /* xyz -> rgb matrix, before scaling to white. */

	    rx = (yg * zb) - (yb * zg);  ry = (xb * zg) - (xg * zb);  rz = (xg * yb) - (xb * yg);
	    gx = (yb * zr) - (yr * zb);  gy = (xr * zb) - (xb * zr);  gz = (xb * yr) - (xr * yb);
	    bx = (yr * zg) - (yg * zr);  by = (xg * zr) - (xr * zg);  bz = (xr * yg) - (xg * yr);

	    /* White scaling factors.
	       Dividing by yw scales the white luminance to unity, as conventional. */

	    rw = ((rx * xw) + (ry * yw) + (rz * zw)) / yw;
	    gw = ((gx * xw) + (gy * yw) + (gz * zw)) / yw;
	    bw = ((bx * xw) + (by * yw) + (bz * zw)) / yw;

	    /* xyz -> rgb matrix, correctly scaled to white. */

	    rx = rx / rw;  ry = ry / rw;  rz = rz / rw;
	    gx = gx / gw;  gy = gy / gw;  gz = gz / gw;
	    bx = bx / bw;  by = by / bw;  bz = bz / bw;

	    /* rgb of the desired point */

	    return new Color(
	    	(float)((rx * xc) + (ry * yc) + (rz * zc)), 
	    	(float)((gx * xc) + (gy * yc) + (gz * zc)), 
	    	(float)((bx * xc) + (by * yc) + (bz * zc)));
	}

	Vector3 spectrum_to_xyz(){
		double X=0,Y=0,Z=0;
		int i=0;
		float lambda = 380f;
		for (; lambda < 780.1f; i++, lambda += 5) {
	        double Me;

	        Me = ColorSpectrum.Evaluate(lambda);
	        if (Me<0)
	        	Me = 0;
	        X += Me * cie_colour_match[i,0];
	        Y += Me * cie_colour_match[i,1];
	        Z += Me * cie_colour_match[i,2];
	    }
	    double XYZ = (X + Y + Z);
	    return new Vector3((float)(X / XYZ), (float)(Y / XYZ), (float)(Z / XYZ));
	}

	Color spectrum_to_rgb(){
		double R=0,G=0,B=0;
		int i=0;
		float lambda = 390;
		for(; lambda <= 730; i++, lambda += 1){
			double Me = ColorSpectrum.Evaluate(lambda);
			if (Me<0)
	        	Me = 0;
	        R += Me * new_rgb_colour_match[i,0];
	        G += Me * new_rgb_colour_match[i,1];
	        B += Me * new_rgb_colour_match[i,2];
		}
		double RGB = Mathf.Max((float)R, (float)G, (float)B);
		RGB = 5;
		return new Color((float)(R/RGB), (float)(G/RGB), (float)(B/RGB));
	}

	Vector4 get_state(float observer_time){
		Vector3 coordinate_to_object_velocity = Velocity;
		Vector3 observer_to_coordinate_velocity = -observerVelocity;
		Matrix4x4 object_to_coordinate_boost = lorentz_boost(-coordinate_to_object_velocity);
		Matrix4x4 coordinate_to_observer_boost = lorentz_boost(-observer_to_coordinate_velocity); //Boost from coordinate frame to observer frame
		Vector4 current_event_object = combine_temporal_and_spatial(-ProperTimeOffset, transform.position);
		Vector4 current_event_coordinate = object_to_coordinate_boost * current_event_object - combine_temporal_and_spatial(0, Observer.transform.position);
		Vector4 current_event_observer = coordinate_to_observer_boost * current_event_coordinate;
		Vector3 observer_to_object_velocity = add_velocity(observer_to_coordinate_velocity, coordinate_to_object_velocity);
		
		List<Vector3> velocities = new List<Vector3>();
		int velocities_index = 0;
		velocities.Add(velocity_to_proper(coordinate_to_object_velocity));
		velocities_index++;
		Proper_Time = 0;
		for (int i=0; i<ProperAccelerations.Count; ++i){
			if (get_temporal_component(current_event_observer) < observer_time){
				Vector3 proper_acceleration = ProperAccelerations[i];
				float proper_duration = AccelerationDurations[i];
				Vector3 offset = AccelerationOffsets[i];
				float a = proper_acceleration.magnitude;
				float L = Vector3.Dot(offset, proper_acceleration)/a;
				if (L <= 1f/a){
					float b = 1f/(1f - a*L);
					proper_acceleration *= b;
					proper_duration /= b;
				}else{
					proper_acceleration = new Vector3(0,0,0);
					proper_duration = 0;
				}
				if (proper_acceleration.magnitude > 0){
					float MCRF_duration = sinh(proper_acceleration.magnitude * proper_duration)/proper_acceleration.magnitude;
					Vector4 next_event_object = combine_temporal_and_spatial(MCRF_duration, get_displacement(proper_acceleration, MCRF_duration));
					Vector4 next_event_coordinate = current_event_coordinate + object_to_coordinate_boost * next_event_object;
					Vector4 next_event_observer = coordinate_to_observer_boost * next_event_coordinate;
					if (get_temporal_component(next_event_observer) > observer_time){
						MCRF_duration = get_MCRF_time(proper_acceleration, -observer_to_coordinate_velocity, object_to_coordinate_boost, current_event_coordinate, observer_time);

						next_event_observer = current_event_observer;
						current_event_object = combine_temporal_and_spatial(MCRF_duration, get_displacement(proper_acceleration, MCRF_duration));
						current_event_coordinate = current_event_coordinate + object_to_coordinate_boost * current_event_object;
						current_event_observer = coordinate_to_observer_boost * current_event_coordinate;
						Vector3 added_velocity = proper_to_velocity(proper_acceleration*MCRF_duration);
						Proper_Time += asinh(proper_acceleration.magnitude * MCRF_duration)/proper_acceleration.magnitude;
						velocities.Add(proper_acceleration*MCRF_duration);
						velocities_index++;
						object_to_coordinate_boost = object_to_coordinate_boost*lorentz_boost(-added_velocity);
						break;
					}else{
						current_event_object = next_event_object;
						current_event_coordinate = next_event_coordinate;
						current_event_observer = next_event_observer;
						Vector3 added_velocity = proper_to_velocity(proper_acceleration*MCRF_duration);
						Proper_Time += asinh(proper_acceleration.magnitude * MCRF_duration)/proper_acceleration.magnitude;
						velocities.Add(proper_acceleration*MCRF_duration);
						velocities_index++;
						object_to_coordinate_boost = object_to_coordinate_boost*lorentz_boost(-added_velocity);
					}
				}else{
					
				}
			}else{
				break;
			}
		}
		Vector3 coordinate_to_object_proper_velocity = velocities[--velocities_index];
		velocities_index--;
		for (int i=velocities_index; i>=0; i--){
			coordinate_to_object_proper_velocity = add_proper_velocity(velocities[i], coordinate_to_object_proper_velocity);
		}
		coordinate_to_object_velocity = proper_to_velocity(coordinate_to_object_proper_velocity);
		observer_to_object_velocity = add_velocity(observer_to_coordinate_velocity, coordinate_to_object_velocity);
		Proper_Time += (observer_time - get_temporal_component(current_event_observer))*α(observer_to_object_velocity);
		current_event_observer += (observer_time - get_temporal_component(current_event_observer))*combine_temporal_and_spatial(1, observer_to_object_velocity);
		Vector4 relative_event_observer = current_event_observer + combine_temporal_and_spatial(0, Observer.transform.position);
		Debug.DrawRay(add_to_Z_axis(get_spatial_component(relative_event_observer), get_temporal_component(relative_event_observer)-observer_time), observer_to_object_velocity, Color.white);
		Current_Velocity = observer_to_object_velocity;
		return current_event_observer + combine_temporal_and_spatial(0, Observer.transform.position);
	}

	void draw_path(float observer_time){
		Vector3 coordinate_to_object_velocity = Velocity;
		Vector3 observer_to_coordinate_velocity = -observerVelocity;
		Matrix4x4 object_to_coordinate_boost = lorentz_boost(-coordinate_to_object_velocity);
		Matrix4x4 coordinate_to_observer_boost = lorentz_boost(-observer_to_coordinate_velocity); //Boost from coordinate frame to observer frame
		Vector4 current_event_object = combine_temporal_and_spatial(-ProperTimeOffset, transform.position);
		Vector4 current_event_coordinate = object_to_coordinate_boost * current_event_object - combine_temporal_and_spatial(0, Observer.transform.position);
		Vector4 current_event_observer = coordinate_to_observer_boost * current_event_coordinate;
		Vector3 observer_to_object_velocity = add_velocity(observer_to_coordinate_velocity, coordinate_to_object_velocity);
		Vector4 relative_event_observer = current_event_observer + combine_temporal_and_spatial(0, Observer.transform.position);
		Debug.DrawRay(add_to_Z_axis(get_spatial_component(relative_event_observer), get_temporal_component(relative_event_observer) - observer_time), -add_to_Z_axis(observer_to_object_velocity, 1)*10000, Color.cyan);
		List<Vector3> velocities = new List<Vector3>();
		int velocities_index = 0;
		velocities.Add(velocity_to_proper(coordinate_to_object_velocity));
		velocities_index++;
		for (int i=0; i<ProperAccelerations.Count; ++i){
			Vector3 proper_acceleration = ProperAccelerations[i];
			float proper_duration = AccelerationDurations[i];
			Vector3 offset = AccelerationOffsets[i];
			float a = proper_acceleration.magnitude;
			float L = Vector3.Dot(offset, proper_acceleration)/a;
			if (L <= 1f/a){
				float b = 1f/(1f - a*L);
				proper_acceleration *= b;
				proper_duration /= b;
			}else{
				proper_acceleration = new Vector3(0,0,0);
				proper_duration = 0;
			}
			if (proper_acceleration.magnitude > 0){
				int count = 1 + (int)AccelerationCurveDetail;
				float increment = proper_duration/count;
				for (int j=0; j<count; ++j){
					float MCRF_time_increment = sinh(proper_acceleration.magnitude * increment)/proper_acceleration.magnitude;
					Vector4 next_event_object = combine_temporal_and_spatial(MCRF_time_increment, get_displacement(proper_acceleration, MCRF_time_increment));
					Vector4 next_event_coordinate = current_event_coordinate + object_to_coordinate_boost * next_event_object;
					Vector4 next_event_observer = coordinate_to_observer_boost * next_event_coordinate;
					relative_event_observer = current_event_observer + combine_temporal_and_spatial(0, Observer.transform.position);
					Vector4 relative_next_event_observer = next_event_observer + combine_temporal_and_spatial(0, Observer.transform.position);
					Debug.DrawLine( add_to_Z_axis(get_spatial_component(relative_event_observer), get_temporal_component(relative_event_observer) - observer_time),
					                add_to_Z_axis(get_spatial_component(relative_next_event_observer), get_temporal_component(relative_next_event_observer) - observer_time),
					                i%2==0?new Color(1,0.5f,0):Color.green);
					current_event_object = next_event_object;
					current_event_coordinate = next_event_coordinate;
					current_event_observer = next_event_observer;
					Vector3 added_velocity = proper_to_velocity(proper_acceleration*MCRF_time_increment);
					//Proper_Time += asinh(proper_acceleration.magnitude * MCRF_time_increment)/proper_acceleration.magnitude;
					velocities.Add(proper_acceleration*MCRF_time_increment);
					velocities_index++;
					object_to_coordinate_boost = object_to_coordinate_boost*lorentz_boost(-added_velocity);
				}
			}
		}
		Vector3 coordinate_to_object_proper_velocity = velocities[--velocities_index];
		velocities_index--;
		for (int i=velocities_index; i>=0; i--){
			coordinate_to_object_proper_velocity = add_proper_velocity(velocities[i], coordinate_to_object_proper_velocity);
		}
		coordinate_to_object_velocity = proper_to_velocity(coordinate_to_object_proper_velocity);
		observer_to_object_velocity = add_velocity(observer_to_coordinate_velocity, coordinate_to_object_velocity);
		relative_event_observer = current_event_observer + combine_temporal_and_spatial(0, Observer.transform.position);
		Debug.DrawRay(add_to_Z_axis(get_spatial_component(relative_event_observer), get_temporal_component(relative_event_observer) - observer_time), add_to_Z_axis(observer_to_object_velocity, 1)*10000, Color.cyan);
	}

	Vector3 velocity_to_proper(Vector3 v){
		return v / Mathf.Sqrt(1f - v.sqrMagnitude);
	}

	Vector3 proper_to_velocity(Vector3 v){
		return v / Mathf.Sqrt(1f + v.sqrMagnitude);
	}

	Vector3 add_velocity(Vector3 v, Vector3 u){
		//Einstein Velocity Addition
		if (v.sqrMagnitude == 0)
			return u;
		if (u.sqrMagnitude == 0)
			return v;
		float gamma = γ(v);
		return 1f/(1+Vector3.Dot(u, v))*(v + u*α(v) + gamma/(1f + gamma)*Vector3.Dot(u, v)*v);
	}

	Vector3 add_proper_velocity(Vector3 v, Vector3 u){
		float Bu = 1/Mathf.Sqrt(1f + u.sqrMagnitude);
		float Bv = 1/Mathf.Sqrt(1f + v.sqrMagnitude);
		return v+u+(Bv/(1+Bv)*Vector3.Dot(v,u) + (1-Bu)/Bu)*v;
	}

	float γ(Vector3 v){
		//Lorentz Factor
		return 1f/Mathf.Sqrt(1 - v.sqrMagnitude);
	}

	float α(Vector3 v){ 
		//Reciprocal Lorentz Factor
		return Mathf.Sqrt(1 - v.sqrMagnitude);
	}

	float sinh(float x)
	{
		return (Mathf.Pow(Mathf.Exp(1), x) - Mathf.Pow(Mathf.Exp(1), -x))/2;
	}

	float cosh(float x)
	{
		return (Mathf.Pow(Mathf.Exp(1), x) + Mathf.Pow(Mathf.Exp(1), -x))/2;
	}

	float tanh(float x)
	{
		return sinh(x)/cosh(x);
	}

	float atanh(float x)
	{
		return (Mathf.Log(1 + x) - Mathf.Log(1 - x))/2;
	}

	float acosh(float x)
	{
		return Mathf.Log(x + Mathf.Sqrt(Mathf.Pow(x,2) - 1));
	}

	float asinh(float x)
	{
		return Mathf.Log(x + Mathf.Sqrt(1 + Mathf.Pow(x,2)));
	}

	Matrix4x4 lorentz_boost(Vector3 v){
		//Computes the Lorentz Boost matrix for a given 3-dimensional velocity
		float βSqr = v.sqrMagnitude;
		if (βSqr == 0f)
		{
			return Matrix4x4.identity;
		}
		float βx = v.x;
		float βy = v.y;
		float βz = v.z;
		float gamma = γ(v);

		Matrix4x4 boost = new Matrix4x4(
			new Vector4(     gamma,  -gamma*βx,                    -gamma*βy,                    -gamma*βz                    ),
			new Vector4( -βx*gamma,  (gamma-1)*(βx*βx)/(βSqr) + 1, (gamma-1)*(βx*βy)/(βSqr),     (gamma-1)*(βx*βz)/(βSqr)     ),
			new Vector4( -βy*gamma,  (gamma-1)*(βy*βx)/(βSqr),     (gamma-1)*(βy*βy)/(βSqr) + 1, (gamma-1)*(βy*βz)/(βSqr)     ),
			new Vector4( -βz*gamma,  (gamma-1)*(βz*βx)/(βSqr),     (gamma-1)*(βz*βy)/(βSqr),     (gamma-1)*(βz*βz)/(βSqr) + 1 )
		);

		return boost;

	}

	Vector4 combine_temporal_and_spatial(float t, Vector3 p){
		return new Vector4(t, p.x, p.y, p.z);
	}

	float get_temporal_component(Vector4 v){
		return v.x;
	}

	Vector3 get_spatial_component(Vector4 v){
		return new Vector3(v.y, v.z, v.w);
	}

	Vector3 add_to_Z_axis(Vector3 v, float z){
		//Add new z value to vector v's z-axis
		v.z += z;
		return v;
	}

	Vector3 boost_to_minkowski(Vector4 pt, Matrix4x4 boost){
		//Applies a Lorentz boost to a (t,x,y,z) point, then converts it into (x,y,t) coordinates for Minkowski diagram rendering
		Vector4 new_pt = boost*pt;
		return new Vector3(new_pt.y, new_pt.z, new_pt.x);
	}

	Vector3 get_displacement(Vector3 a, float T){
		return
		(
			a
			*
			(
				Mathf.Sqrt(
					1
					+
					Mathf.Pow(T, 2)
					*
					a.sqrMagnitude
				)
				-
				1
			)
		)
		/
		(
			a.sqrMagnitude
		);
	}

	float get_observer_time(Vector3 a, Vector3 cV, Matrix4x4 object_to_coordinate_boost, Vector4 current_event_coordinate, float MCRFTime){
		float ax = a.x;
		float ay = a.y;
		float az = a.z;
		float currCoordX = get_spatial_component(current_event_coordinate).x;
		float currCoordY = get_spatial_component(current_event_coordinate).y;
		float currCoordZ = get_spatial_component(current_event_coordinate).z;
		float currCoordT = get_temporal_component(current_event_coordinate);
		float cVx = cV.x;
		float cVy = cV.y;
		float cVz = cV.z;
		float B11 = object_to_coordinate_boost[0,0];
		float B12 = object_to_coordinate_boost[0,1];
		float B13 = object_to_coordinate_boost[0,2];
		float B14 = object_to_coordinate_boost[0,3];
		float B21 = object_to_coordinate_boost[1,0];
		float B22 = object_to_coordinate_boost[1,1];
		float B23 = object_to_coordinate_boost[1,2];
		float B24 = object_to_coordinate_boost[1,3];
		float B31 = object_to_coordinate_boost[2,0];
		float B32 = object_to_coordinate_boost[2,1];
		float B33 = object_to_coordinate_boost[2,2];
		float B34 = object_to_coordinate_boost[2,3];
		float B41 = object_to_coordinate_boost[3,0];
		float B42 = object_to_coordinate_boost[3,1];
		float B43 = object_to_coordinate_boost[3,2];
		float B44 = object_to_coordinate_boost[3,3];
		
		return
		(1/((Mathf.Pow(ax, 2) + Mathf.Pow(ay, 2) + Mathf.Pow(az, 2))*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))))*((-az)*B14 + Mathf.Pow(ax, 2)*(currCoordT - currCoordX*cVx - currCoordY*cVy - currCoordZ*cVz + B11*MCRFTime - (B21*cVx + B31*cVy + B41*cVz)*MCRFTime) + 
		Mathf.Pow(ay, 2)*(currCoordT - currCoordX*cVx - currCoordY*cVy - currCoordZ*cVz + B11*MCRFTime - (B21*cVx + B31*cVy + B41*cVz)*MCRFTime) + ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(-1 + Mathf.Sqrt(1 + (Mathf.Pow(ax, 2) + Mathf.Pow(ay, 2) + Mathf.Pow(az, 2))*Mathf.Pow(MCRFTime, 2))) + 
		ay*(B13 - B23*cVx - B33*cVy - B43*cVz)*(-1 + Mathf.Sqrt(1 + (Mathf.Pow(ax, 2) + Mathf.Pow(ay, 2) + Mathf.Pow(az, 2))*Mathf.Pow(MCRFTime, 2))) + 
		az*(B24*cVx + B34*cVy + B44*cVz + az*(currCoordT - currCoordX*cVx - currCoordY*cVy - currCoordZ*cVz + B11*MCRFTime - (B21*cVx + B31*cVy + B41*cVz)*MCRFTime) + B14*Mathf.Sqrt(1 + (Mathf.Pow(ax, 2) + Mathf.Pow(ay, 2) + Mathf.Pow(az, 2))*Mathf.Pow(MCRFTime, 2)) - 
		(B24*cVx + B34*cVy + B44*cVz)*Mathf.Sqrt(1 + (Mathf.Pow(ax, 2) + Mathf.Pow(ay, 2) + Mathf.Pow(az, 2))*Mathf.Pow(MCRFTime, 2))));
	}

	float get_MCRF_time(Vector3 a, Vector3 coordinate_velocity, Matrix4x4 object_to_coordinate_boost, Vector4 current_event_coordinate, float observer_time){
		//Inverse of get_observer_time(). Outputted using Mathematica (Sorry!). Inverse has two solutions, check which is valid then return. Again, sorry for this. I feel really bad.
		//This is actually the ugliest thing I have ever made. So sorry.
		float currCoordT = get_temporal_component(current_event_coordinate);
		float currCoordX = get_spatial_component(current_event_coordinate).x;
		float currCoordY = get_spatial_component(current_event_coordinate).y;
		float currCoordZ = get_spatial_component(current_event_coordinate).z;
		float ax = a.x;
		float ay = a.y;
		float az = a.z;
		float cVx = coordinate_velocity.x;
		float cVy = coordinate_velocity.y;
		float cVz = coordinate_velocity.z;
		float B11 = object_to_coordinate_boost[0,0];
		float B12 = object_to_coordinate_boost[0,1];
		float B13 = object_to_coordinate_boost[0,2];
		float B14 = object_to_coordinate_boost[0,3];
		float B21 = object_to_coordinate_boost[1,0];
		float B22 = object_to_coordinate_boost[1,1];
		float B23 = object_to_coordinate_boost[1,2];
		float B24 = object_to_coordinate_boost[1,3];
		float B31 = object_to_coordinate_boost[2,0];
		float B32 = object_to_coordinate_boost[2,1];
		float B33 = object_to_coordinate_boost[2,2];
		float B34 = object_to_coordinate_boost[2,3];
		float B41 = object_to_coordinate_boost[3,0];
		float B42 = object_to_coordinate_boost[3,1];
		float B43 = object_to_coordinate_boost[3,2];
		float B44 = object_to_coordinate_boost[3,3];
		float t = observer_time;
		float T1 = (Mathf.Sqrt(Mathf.Pow(ax*(-B12 + B22*cVx + B32*cVy + B42*cVz) + ay*(-B13 + B23*cVx + B33*cVy + B43*cVz) + az*(-B14 + B24*cVx + B34*cVy + B44*cVz), 2)*(Mathf.Pow(B11, 2) - 2*az*B14*currCoordT + Mathf.Pow(az, 2)*Mathf.Pow(currCoordT, 2) + 2*az*B24*currCoordT*cVx + 
		           2*az*B14*currCoordX*cVx - 2*Mathf.Pow(az, 2)*currCoordT*currCoordX*cVx + Mathf.Pow(B21, 2)*Mathf.Pow(cVx, 2) - 2*az*B24*currCoordX*Mathf.Pow(cVx, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordX, 2)*Mathf.Pow(cVx, 2) + 2*az*B34*currCoordT*cVy + 2*az*B14*currCoordY*cVy - 2*Mathf.Pow(az, 2)*currCoordT*currCoordY*cVy + 
		           2*B21*B31*cVx*cVy - 2*az*B34*currCoordX*cVx*cVy - 2*az*B24*currCoordY*cVx*cVy + 2*Mathf.Pow(az, 2)*currCoordX*currCoordY*cVx*cVy + Mathf.Pow(B31, 2)*Mathf.Pow(cVy, 2) - 2*az*B34*currCoordY*Mathf.Pow(cVy, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordY, 2)*Mathf.Pow(cVy, 2) + 2*az*B44*currCoordT*cVz + 
		           2*az*B14*currCoordZ*cVz - 2*Mathf.Pow(az, 2)*currCoordT*currCoordZ*cVz + 2*B21*B41*cVx*cVz - 2*az*B44*currCoordX*cVx*cVz - 2*az*B24*currCoordZ*cVx*cVz + 2*Mathf.Pow(az, 2)*currCoordX*currCoordZ*cVx*cVz + 2*B31*B41*cVy*cVz - 
		           2*az*B44*currCoordY*cVy*cVz - 2*az*B34*currCoordZ*cVy*cVz + 2*Mathf.Pow(az, 2)*currCoordY*currCoordZ*cVy*cVz + Mathf.Pow(B41, 2)*Mathf.Pow(cVz, 2) - 2*az*B44*currCoordZ*Mathf.Pow(cVz, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordZ, 2)*Mathf.Pow(cVz, 2) - 2*B11*(B21*cVx + B31*cVy + B41*cVz) + 
		           2*ay*(B13 - B23*cVx - B33*cVy - B43*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz) + 2*ay*B13*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*az*B14*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 
		           2*Mathf.Pow(ay, 2)*currCoordT*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*Mathf.Pow(az, 2)*currCoordT*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*ay*B23*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*az*B24*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 
		           2*Mathf.Pow(ay, 2)*currCoordX*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordX*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*ay*B33*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*az*B34*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 
		           2*Mathf.Pow(ay, 2)*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*ay*B43*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*az*B44*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 
		           2*Mathf.Pow(ay, 2)*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + Mathf.Pow(az, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVx, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVy, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVz, 2)*Mathf.Pow(t, 2) + 
		           2*ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + 
		           Mathf.Pow(ay, 2)*(Mathf.Pow(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz, 2) - (-1 + Mathf.Pow(cVx, 2) + Mathf.Pow(cVy, 2) + Mathf.Pow(cVz, 2))*Mathf.Pow(t, 2)) + Mathf.Pow(ax, 2)*(Mathf.Pow(currCoordT, 2) + Mathf.Pow(currCoordX, 2)*Mathf.Pow(cVx, 2) + Mathf.Pow(currCoordY, 2)*Mathf.Pow(cVy, 2) + 2*currCoordY*currCoordZ*cVy*cVz + 
		           Mathf.Pow(currCoordZ, 2)*Mathf.Pow(cVz, 2) + 2*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + Mathf.Pow(t, 2) - Mathf.Pow(cVx, 2)*Mathf.Pow(t, 2) - Mathf.Pow(cVy, 2)*Mathf.Pow(t, 2) - Mathf.Pow(cVz, 2)*Mathf.Pow(t, 2) + 
		           2*currCoordX*cVx*(currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) - 2*currCoordT*(currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t)))) + 
		           (B11 - B21*cVx - B31*cVy - B41*cVz)*(ax*(B12 - B22*cVx - B32*cVy - B42*cVz) + ay*(B13 - B23*cVx - B33*cVy - B43*cVz) + Mathf.Pow(ax, 2)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + 
		           Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + Mathf.Pow(ay, 2)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + 
		           az*(B14 - B24*cVx - B34*cVy - B44*cVz + az*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t))))/
		           (Mathf.Pow(ax, 2)*(B11 - B12 - B21*cVx + B22*cVx - B31*cVy + B32*cVy - B41*cVz + B42*cVz)*(B11 + B12 - (B21 + B22)*cVx - (B31 + B32)*cVy - (B41 + B42)*cVz) + 
		           Mathf.Pow(ay, 2)*(B11 - B13 - B21*cVx + B23*cVx - B31*cVy + B33*cVy - B41*cVz + B43*cVz)*(B11 + B13 - (B21 + B23)*cVx - (B31 + B33)*cVy - (B41 + B43)*cVz) + 
		           2*ay*az*(B13 - B23*cVx - B33*cVy - B43*cVz)*(-B14 + B24*cVx + B34*cVy + B44*cVz) + Mathf.Pow(az, 2)*(B11 - B14 - B21*cVx + B24*cVx - B31*cVy + B34*cVy - B41*cVz + B44*cVz)*
		           (B11 + B14 - (B21 + B24)*cVx - (B31 + B34)*cVy - (B41 + B44)*cVz) + 2*ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(ay*(-B13 + B23*cVx + B33*cVy + B43*cVz) + az*(-B14 + B24*cVx + B34*cVy + B44*cVz)));
		float T2 = (-Mathf.Sqrt(Mathf.Pow(ax*(-B12 + B22*cVx + B32*cVy + B42*cVz) + ay*(-B13 + B23*cVx + B33*cVy + B43*cVz) + az*(-B14 + B24*cVx + B34*cVy + B44*cVz), 2)*(Mathf.Pow(B11, 2) - 2*az*B14*currCoordT + Mathf.Pow(az, 2)*Mathf.Pow(currCoordT, 2) + 2*az*B24*currCoordT*cVx + 
		           2*az*B14*currCoordX*cVx - 2*Mathf.Pow(az, 2)*currCoordT*currCoordX*cVx + Mathf.Pow(B21, 2)*Mathf.Pow(cVx, 2) - 2*az*B24*currCoordX*Mathf.Pow(cVx, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordX, 2)*Mathf.Pow(cVx, 2) + 2*az*B34*currCoordT*cVy + 2*az*B14*currCoordY*cVy - 2*Mathf.Pow(az, 2)*currCoordT*currCoordY*cVy + 
		           2*B21*B31*cVx*cVy - 2*az*B34*currCoordX*cVx*cVy - 2*az*B24*currCoordY*cVx*cVy + 2*Mathf.Pow(az, 2)*currCoordX*currCoordY*cVx*cVy + Mathf.Pow(B31, 2)*Mathf.Pow(cVy, 2) - 2*az*B34*currCoordY*Mathf.Pow(cVy, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordY, 2)*Mathf.Pow(cVy, 2) + 2*az*B44*currCoordT*cVz + 
		           2*az*B14*currCoordZ*cVz - 2*Mathf.Pow(az, 2)*currCoordT*currCoordZ*cVz + 2*B21*B41*cVx*cVz - 2*az*B44*currCoordX*cVx*cVz - 2*az*B24*currCoordZ*cVx*cVz + 2*Mathf.Pow(az, 2)*currCoordX*currCoordZ*cVx*cVz + 2*B31*B41*cVy*cVz - 
		           2*az*B44*currCoordY*cVy*cVz - 2*az*B34*currCoordZ*cVy*cVz + 2*Mathf.Pow(az, 2)*currCoordY*currCoordZ*cVy*cVz + Mathf.Pow(B41, 2)*Mathf.Pow(cVz, 2) - 2*az*B44*currCoordZ*Mathf.Pow(cVz, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordZ, 2)*Mathf.Pow(cVz, 2) - 2*B11*(B21*cVx + B31*cVy + B41*cVz) + 
		           2*ay*(B13 - B23*cVx - B33*cVy - B43*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz) + 2*ay*B13*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*az*B14*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 
		           2*Mathf.Pow(ay, 2)*currCoordT*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*Mathf.Pow(az, 2)*currCoordT*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*ay*B23*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*az*B24*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 
		           2*Mathf.Pow(ay, 2)*currCoordX*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordX*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*ay*B33*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*az*B34*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 
		           2*Mathf.Pow(ay, 2)*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*ay*B43*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*az*B44*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 
		           2*Mathf.Pow(ay, 2)*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + Mathf.Pow(az, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVx, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVy, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVz, 2)*Mathf.Pow(t, 2) + 
		           2*ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + 
		           Mathf.Pow(ay, 2)*(Mathf.Pow(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz, 2) - (-1 + Mathf.Pow(cVx, 2) + Mathf.Pow(cVy, 2) + Mathf.Pow(cVz, 2))*Mathf.Pow(t, 2)) + Mathf.Pow(ax, 2)*(Mathf.Pow(currCoordT, 2) + Mathf.Pow(currCoordX, 2)*Mathf.Pow(cVx, 2) + Mathf.Pow(currCoordY, 2)*Mathf.Pow(cVy, 2) + 2*currCoordY*currCoordZ*cVy*cVz + 
		           Mathf.Pow(currCoordZ, 2)*Mathf.Pow(cVz, 2) + 2*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + Mathf.Pow(t, 2) - Mathf.Pow(cVx, 2)*Mathf.Pow(t, 2) - Mathf.Pow(cVy, 2)*Mathf.Pow(t, 2) - Mathf.Pow(cVz, 2)*Mathf.Pow(t, 2) + 
		           2*currCoordX*cVx*(currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) - 2*currCoordT*(currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t)))) + 
		           (B11 - B21*cVx - B31*cVy - B41*cVz)*(ax*(B12 - B22*cVx - B32*cVy - B42*cVz) + ay*(B13 - B23*cVx - B33*cVy - B43*cVz) + Mathf.Pow(ax, 2)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + 
		           Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + Mathf.Pow(ay, 2)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + 
		           az*(B14 - B24*cVx - B34*cVy - B44*cVz + az*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t))))/
		           (Mathf.Pow(ax, 2)*(B11 - B12 - B21*cVx + B22*cVx - B31*cVy + B32*cVy - B41*cVz + B42*cVz)*(B11 + B12 - (B21 + B22)*cVx - (B31 + B32)*cVy - (B41 + B42)*cVz) + 
		           Mathf.Pow(ay, 2)*(B11 - B13 - B21*cVx + B23*cVx - B31*cVy + B33*cVy - B41*cVz + B43*cVz)*(B11 + B13 - (B21 + B23)*cVx - (B31 + B33)*cVy - (B41 + B43)*cVz) + 
		           2*ay*az*(B13 - B23*cVx - B33*cVy - B43*cVz)*(-B14 + B24*cVx + B34*cVy + B44*cVz) + Mathf.Pow(az, 2)*(B11 - B14 - B21*cVx + B24*cVx - B31*cVy + B34*cVy - B41*cVz + B44*cVz)*
		           (B11 + B14 - (B21 + B24)*cVx - (B31 + B34)*cVy - (B41 + B44)*cVz) + 2*ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(ay*(-B13 + B23*cVx + B33*cVy + B43*cVz) + az*(-B14 + B24*cVx + B34*cVy + B44*cVz)));
		/*
		float T1 = (az*B11*B14 - Mathf.Pow(az, 2)*B11*currCoordT - az*B14*B21*cVx - az*B11*B24*cVx + Mathf.Pow(az, 2)*B21*currCoordT*cVx + Mathf.Pow(az, 2)*B11*currCoordX*cVx + az*B21*B24*Mathf.Pow(cVx, 2) - Mathf.Pow(az, 2)*B21*currCoordX*Mathf.Pow(cVx, 2) - az*B14*B31*cVy - az*B11*B34*cVy + 
		           Mathf.Pow(az, 2)*B31*currCoordT*cVy + Mathf.Pow(az, 2)*B11*currCoordY*cVy + az*B24*B31*cVx*cVy + az*B21*B34*cVx*cVy - Mathf.Pow(az, 2)*B31*currCoordX*cVx*cVy - Mathf.Pow(az, 2)*B21*currCoordY*cVx*cVy + az*B31*B34*Mathf.Pow(cVy, 2) - Mathf.Pow(az, 2)*B31*currCoordY*Mathf.Pow(cVy, 2) - az*B14*B41*cVz - 
		           az*B11*B44*cVz + Mathf.Pow(az, 2)*B41*currCoordT*cVz + Mathf.Pow(az, 2)*B11*currCoordZ*cVz + az*B24*B41*cVx*cVz + az*B21*B44*cVx*cVz - Mathf.Pow(az, 2)*B41*currCoordX*cVx*cVz - Mathf.Pow(az, 2)*B21*currCoordZ*cVx*cVz + az*B34*B41*cVy*cVz + az*B31*B44*cVy*cVz - 
		           Mathf.Pow(az, 2)*B41*currCoordY*cVy*cVz - Mathf.Pow(az, 2)*B31*currCoordZ*cVy*cVz + az*B41*B44*Mathf.Pow(cVz, 2) - Mathf.Pow(az, 2)*B41*currCoordZ*Mathf.Pow(cVz, 2) - ax*(B11 - B21*cVx - B31*cVy - B41*cVz)*(-B12 + B22*cVx + B32*cVy + B42*cVz) - 
		           ay*(B11 - B21*cVx - B31*cVy - B41*cVz)*(-B13 + B23*cVx + B33*cVy + B43*cVz) + Mathf.Pow(az, 2)*B11*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - Mathf.Pow(az, 2)*B21*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - Mathf.Pow(az, 2)*B31*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 
		           Mathf.Pow(az, 2)*B41*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + Mathf.Pow(ax, 2)*(B11 - B21*cVx - B31*cVy - B41*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + 
		           Mathf.Pow(ay, 2)*(B11 - B21*cVx - B31*cVy - B41*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + 
		           Mathf.Sqrt(Mathf.Pow(ax*(-B12 + B22*cVx + B32*cVy + B42*cVz) + ay*(-B13 + B23*cVx + B33*cVy + B43*cVz) + az*(-B14 + B24*cVx + B34*cVy + B44*cVz), 2)*(Mathf.Pow(B11, 2) - 2*ay*B13*currCoordT - 2*az*B14*currCoordT + Mathf.Pow(ay, 2)*Mathf.Pow(currCoordT, 2) + 
		           Mathf.Pow(az, 2)*Mathf.Pow(currCoordT, 2) + 2*ay*B23*currCoordT*cVx + 2*az*B24*currCoordT*cVx + 2*ay*B13*currCoordX*cVx + 2*az*B14*currCoordX*cVx - 2*Mathf.Pow(ay, 2)*currCoordT*currCoordX*cVx - 2*Mathf.Pow(az, 2)*currCoordT*currCoordX*cVx + Mathf.Pow(B21, 2)*Mathf.Pow(cVx, 2) - 
		           2*ay*B23*currCoordX*Mathf.Pow(cVx, 2) - 2*az*B24*currCoordX*Mathf.Pow(cVx, 2) + Mathf.Pow(ay, 2)*Mathf.Pow(currCoordX, 2)*Mathf.Pow(cVx, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordX, 2)*Mathf.Pow(cVx, 2) + 2*ay*B33*currCoordT*cVy + 2*az*B34*currCoordT*cVy + 2*ay*B13*currCoordY*cVy + 2*az*B14*currCoordY*cVy - 
		           2*Mathf.Pow(ay, 2)*currCoordT*currCoordY*cVy - 2*Mathf.Pow(az, 2)*currCoordT*currCoordY*cVy + 2*B21*B31*cVx*cVy - 2*ay*B33*currCoordX*cVx*cVy - 2*az*B34*currCoordX*cVx*cVy - 2*ay*B23*currCoordY*cVx*cVy - 2*az*B24*currCoordY*cVx*cVy + 
		           2*Mathf.Pow(ay, 2)*currCoordX*currCoordY*cVx*cVy + 2*Mathf.Pow(az, 2)*currCoordX*currCoordY*cVx*cVy + Mathf.Pow(B31, 2)*Mathf.Pow(cVy, 2) - 2*ay*B33*currCoordY*Mathf.Pow(cVy, 2) - 2*az*B34*currCoordY*Mathf.Pow(cVy, 2) + Mathf.Pow(ay, 2)*Mathf.Pow(currCoordY, 2)*Mathf.Pow(cVy, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordY, 2)*Mathf.Pow(cVy, 2) + 
		           2*ay*B43*currCoordT*cVz + 2*az*B44*currCoordT*cVz + 2*ay*B13*currCoordZ*cVz + 2*az*B14*currCoordZ*cVz - 2*Mathf.Pow(ay, 2)*currCoordT*currCoordZ*cVz - 2*Mathf.Pow(az, 2)*currCoordT*currCoordZ*cVz + 2*B21*B41*cVx*cVz - 
		           2*ay*B43*currCoordX*cVx*cVz - 2*az*B44*currCoordX*cVx*cVz - 2*ay*B23*currCoordZ*cVx*cVz - 2*az*B24*currCoordZ*cVx*cVz + 2*Mathf.Pow(ay, 2)*currCoordX*currCoordZ*cVx*cVz + 2*Mathf.Pow(az, 2)*currCoordX*currCoordZ*cVx*cVz + 2*B31*B41*cVy*cVz - 
		           2*ay*B43*currCoordY*cVy*cVz - 2*az*B44*currCoordY*cVy*cVz - 2*ay*B33*currCoordZ*cVy*cVz - 2*az*B34*currCoordZ*cVy*cVz + 2*Mathf.Pow(ay, 2)*currCoordY*currCoordZ*cVy*cVz + 2*Mathf.Pow(az, 2)*currCoordY*currCoordZ*cVy*cVz + Mathf.Pow(B41, 2)*Mathf.Pow(cVz, 2) - 
		           2*ay*B43*currCoordZ*Mathf.Pow(cVz, 2) - 2*az*B44*currCoordZ*Mathf.Pow(cVz, 2) + Mathf.Pow(ay, 2)*Mathf.Pow(currCoordZ, 2)*Mathf.Pow(cVz, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordZ, 2)*Mathf.Pow(cVz, 2) - 2*B11*(B21*cVx + B31*cVy + B41*cVz) + 2*ay*B13*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 
		           2*az*B14*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*Mathf.Pow(ay, 2)*currCoordT*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*Mathf.Pow(az, 2)*currCoordT*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*ay*B23*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 
		           2*az*B24*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(ay, 2)*currCoordX*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordX*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*ay*B33*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 
		           2*az*B34*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(ay, 2)*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*ay*B43*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 
		           2*az*B44*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(ay, 2)*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + Mathf.Pow(ay, 2)*Mathf.Pow(t, 2) + Mathf.Pow(az, 2)*Mathf.Pow(t, 2) - Mathf.Pow(ay, 2)*Mathf.Pow(cVx, 2)*Mathf.Pow(t, 2) - 
		           Mathf.Pow(az, 2)*Mathf.Pow(cVx, 2)*Mathf.Pow(t, 2) - Mathf.Pow(ay, 2)*Mathf.Pow(cVy, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVy, 2)*Mathf.Pow(t, 2) - Mathf.Pow(ay, 2)*Mathf.Pow(cVz, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVz, 2)*Mathf.Pow(t, 2) + 2*ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + 
		           Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + Mathf.Pow(ax, 2)*(Mathf.Pow(currCoordT, 2) + Mathf.Pow(currCoordX, 2)*Mathf.Pow(cVx, 2) + Mathf.Pow(currCoordY, 2)*Mathf.Pow(cVy, 2) + 2*currCoordY*currCoordZ*cVy*cVz + Mathf.Pow(currCoordZ, 2)*Mathf.Pow(cVz, 2) + 2*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 
		           2*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + Mathf.Pow(t, 2) - Mathf.Pow(cVx, 2)*Mathf.Pow(t, 2) - Mathf.Pow(cVy, 2)*Mathf.Pow(t, 2) - Mathf.Pow(cVz, 2)*Mathf.Pow(t, 2) + 2*currCoordX*cVx*(currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) - 
		           2*currCoordT*(currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t)))))/(2*ay*az*(B13 - B23*cVx - B33*cVy - B43*cVz)*(-B14 + B24*cVx + B34*cVy + B44*cVz) + 
		           Mathf.Pow(ax, 2)*(Mathf.Pow(B11, 2) - Mathf.Pow(B12, 2) + Mathf.Pow(B21, 2)*Mathf.Pow(cVx, 2) - Mathf.Pow(B22, 2)*Mathf.Pow(cVx, 2) + 2*B21*B31*cVx*cVy - 2*B22*B32*cVx*cVy + Mathf.Pow(B31, 2)*Mathf.Pow(cVy, 2) - Mathf.Pow(B32, 2)*Mathf.Pow(cVy, 2) + 2*B21*B41*cVx*cVz - 2*B22*B42*cVx*cVz + 2*B31*B41*cVy*cVz - 2*B32*B42*cVy*cVz + Mathf.Pow(B41, 2)*Mathf.Pow(cVz, 2) - 
		           Mathf.Pow(B42, 2)*Mathf.Pow(cVz, 2) - 2*B11*(B21*cVx + B31*cVy + B41*cVz) + 2*B12*(B22*cVx + B32*cVy + B42*cVz)) + Mathf.Pow(ay, 2)*(Mathf.Pow(B11, 2) - Mathf.Pow(B13, 2) + Mathf.Pow(B21, 2)*Mathf.Pow(cVx, 2) - Mathf.Pow(B23, 2)*Mathf.Pow(cVx, 2) + 2*B21*B31*cVx*cVy - 2*B23*B33*cVx*cVy + Mathf.Pow(B31, 2)*Mathf.Pow(cVy, 2) - Mathf.Pow(B33, 2)*Mathf.Pow(cVy, 2) + 
		           2*B21*B41*cVx*cVz - 2*B23*B43*cVx*cVz + 2*B31*B41*cVy*cVz - 2*B33*B43*cVy*cVz + Mathf.Pow(B41, 2)*Mathf.Pow(cVz, 2) - Mathf.Pow(B43, 2)*Mathf.Pow(cVz, 2) - 2*B11*(B21*cVx + B31*cVy + B41*cVz) + 2*B13*(B23*cVx + B33*cVy + B43*cVz)) + 
		           Mathf.Pow(az, 2)*(Mathf.Pow(B11, 2) - Mathf.Pow(B14, 2) + Mathf.Pow(B21, 2)*Mathf.Pow(cVx, 2) - Mathf.Pow(B24, 2)*Mathf.Pow(cVx, 2) + 2*B21*B31*cVx*cVy - 2*B24*B34*cVx*cVy + Mathf.Pow(B31, 2)*Mathf.Pow(cVy, 2) - Mathf.Pow(B34, 2)*Mathf.Pow(cVy, 2) + 2*B21*B41*cVx*cVz - 2*B24*B44*cVx*cVz + 2*B31*B41*cVy*cVz - 2*B34*B44*cVy*cVz + Mathf.Pow(B41, 2)*Mathf.Pow(cVz, 2) - 
		           Mathf.Pow(B44, 2)*Mathf.Pow(cVz, 2) - 2*B11*(B21*cVx + B31*cVy + B41*cVz) + 2*B14*(B24*cVx + B34*cVy + B44*cVz)) + 2*ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(ay*(-B13 + B23*cVx + B33*cVy + B43*cVz) + az*(-B14 + B24*cVx + B34*cVy + B44*cVz)));

		float T2 = 	((-az)*B11*B14 + Mathf.Pow(az, 2)*B11*currCoordT + az*B14*B21*cVx + az*B11*B24*cVx - Mathf.Pow(az, 2)*B21*currCoordT*cVx - Mathf.Pow(az, 2)*B11*currCoordX*cVx - az*B21*B24*Mathf.Pow(cVx, 2) + Mathf.Pow(az, 2)*B21*currCoordX*Mathf.Pow(cVx, 2) + az*B14*B31*cVy + az*B11*B34*cVy - 
		           Mathf.Pow(az, 2)*B31*currCoordT*cVy - Mathf.Pow(az, 2)*B11*currCoordY*cVy - az*B24*B31*cVx*cVy - az*B21*B34*cVx*cVy + Mathf.Pow(az, 2)*B31*currCoordX*cVx*cVy + Mathf.Pow(az, 2)*B21*currCoordY*cVx*cVy - az*B31*B34*Mathf.Pow(cVy, 2) + Mathf.Pow(az, 2)*B31*currCoordY*Mathf.Pow(cVy, 2) + az*B14*B41*cVz + 
		           az*B11*B44*cVz - Mathf.Pow(az, 2)*B41*currCoordT*cVz - Mathf.Pow(az, 2)*B11*currCoordZ*cVz - az*B24*B41*cVx*cVz - az*B21*B44*cVx*cVz + Mathf.Pow(az, 2)*B41*currCoordX*cVx*cVz + Mathf.Pow(az, 2)*B21*currCoordZ*cVx*cVz - az*B34*B41*cVy*cVz - az*B31*B44*cVy*cVz + 
		           Mathf.Pow(az, 2)*B41*currCoordY*cVy*cVz + Mathf.Pow(az, 2)*B31*currCoordZ*cVy*cVz - az*B41*B44*Mathf.Pow(cVz, 2) + Mathf.Pow(az, 2)*B41*currCoordZ*Mathf.Pow(cVz, 2) + ax*(B11 - B21*cVx - B31*cVy - B41*cVz)*(-B12 + B22*cVx + B32*cVy + B42*cVz) + 
		           ay*(B11 - B21*cVx - B31*cVy - B41*cVz)*(-B13 + B23*cVx + B33*cVy + B43*cVz) - Mathf.Pow(az, 2)*B11*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + Mathf.Pow(az, 2)*B21*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 
		           Mathf.Pow(az, 2)*B31*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + Mathf.Pow(az, 2)*B41*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - Mathf.Pow(ax, 2)*(B11 - B21*cVx - B31*cVy - B41*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + 
		           Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) - Mathf.Pow(ay, 2)*(B11 - B21*cVx - B31*cVy - B41*cVz)*(-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + 
		           Mathf.Sqrt(Mathf.Pow(ax*(-B12 + B22*cVx + B32*cVy + B42*cVz) + ay*(-B13 + B23*cVx + B33*cVy + B43*cVz) + az*(-B14 + B24*cVx + B34*cVy + B44*cVz), 2)*(Mathf.Pow(B11, 2) - 2*ay*B13*currCoordT - 2*az*B14*currCoordT + Mathf.Pow(ay, 2)*Mathf.Pow(currCoordT, 2) + 
		           Mathf.Pow(az, 2)*Mathf.Pow(currCoordT, 2) + 2*ay*B23*currCoordT*cVx + 2*az*B24*currCoordT*cVx + 2*ay*B13*currCoordX*cVx + 2*az*B14*currCoordX*cVx - 2*Mathf.Pow(ay, 2)*currCoordT*currCoordX*cVx - 2*Mathf.Pow(az, 2)*currCoordT*currCoordX*cVx + Mathf.Pow(B21, 2)*Mathf.Pow(cVx, 2) - 
		           2*ay*B23*currCoordX*Mathf.Pow(cVx, 2) - 2*az*B24*currCoordX*Mathf.Pow(cVx, 2) + Mathf.Pow(ay, 2)*Mathf.Pow(currCoordX, 2)*Mathf.Pow(cVx, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordX, 2)*Mathf.Pow(cVx, 2) + 2*ay*B33*currCoordT*cVy + 2*az*B34*currCoordT*cVy + 2*ay*B13*currCoordY*cVy + 2*az*B14*currCoordY*cVy - 
		           2*Mathf.Pow(ay, 2)*currCoordT*currCoordY*cVy - 2*Mathf.Pow(az, 2)*currCoordT*currCoordY*cVy + 2*B21*B31*cVx*cVy - 2*ay*B33*currCoordX*cVx*cVy - 2*az*B34*currCoordX*cVx*cVy - 2*ay*B23*currCoordY*cVx*cVy - 2*az*B24*currCoordY*cVx*cVy + 
		           2*Mathf.Pow(ay, 2)*currCoordX*currCoordY*cVx*cVy + 2*Mathf.Pow(az, 2)*currCoordX*currCoordY*cVx*cVy + Mathf.Pow(B31, 2)*Mathf.Pow(cVy, 2) - 2*ay*B33*currCoordY*Mathf.Pow(cVy, 2) - 2*az*B34*currCoordY*Mathf.Pow(cVy, 2) + Mathf.Pow(ay, 2)*Mathf.Pow(currCoordY, 2)*Mathf.Pow(cVy, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordY, 2)*Mathf.Pow(cVy, 2) + 
		           2*ay*B43*currCoordT*cVz + 2*az*B44*currCoordT*cVz + 2*ay*B13*currCoordZ*cVz + 2*az*B14*currCoordZ*cVz - 2*Mathf.Pow(ay, 2)*currCoordT*currCoordZ*cVz - 2*Mathf.Pow(az, 2)*currCoordT*currCoordZ*cVz + 2*B21*B41*cVx*cVz - 
		           2*ay*B43*currCoordX*cVx*cVz - 2*az*B44*currCoordX*cVx*cVz - 2*ay*B23*currCoordZ*cVx*cVz - 2*az*B24*currCoordZ*cVx*cVz + 2*Mathf.Pow(ay, 2)*currCoordX*currCoordZ*cVx*cVz + 2*Mathf.Pow(az, 2)*currCoordX*currCoordZ*cVx*cVz + 
		           2*B31*B41*cVy*cVz - 2*ay*B43*currCoordY*cVy*cVz - 2*az*B44*currCoordY*cVy*cVz - 2*ay*B33*currCoordZ*cVy*cVz - 2*az*B34*currCoordZ*cVy*cVz + 2*Mathf.Pow(ay, 2)*currCoordY*currCoordZ*cVy*cVz + 
		           2*Mathf.Pow(az, 2)*currCoordY*currCoordZ*cVy*cVz + Mathf.Pow(B41, 2)*Mathf.Pow(cVz, 2) - 2*ay*B43*currCoordZ*Mathf.Pow(cVz, 2) - 2*az*B44*currCoordZ*Mathf.Pow(cVz, 2) + Mathf.Pow(ay, 2)*Mathf.Pow(currCoordZ, 2)*Mathf.Pow(cVz, 2) + Mathf.Pow(az, 2)*Mathf.Pow(currCoordZ, 2)*Mathf.Pow(cVz, 2) - 2*B11*(B21*cVx + B31*cVy + B41*cVz) + 
		           2*ay*B13*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*az*B14*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*Mathf.Pow(ay, 2)*currCoordT*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*Mathf.Pow(az, 2)*currCoordT*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 
		           2*ay*B23*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*az*B24*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(ay, 2)*currCoordX*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordX*cVx*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 
		           2*ay*B33*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*az*B34*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(ay, 2)*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 
		           2*ay*B43*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t - 2*az*B44*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(ay, 2)*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*Mathf.Pow(az, 2)*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 
		           Mathf.Pow(ay, 2)*Mathf.Pow(t, 2) + Mathf.Pow(az, 2)*Mathf.Pow(t, 2) - Mathf.Pow(ay, 2)*Mathf.Pow(cVx, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVx, 2)*Mathf.Pow(t, 2) - Mathf.Pow(ay, 2)*Mathf.Pow(cVy, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVy, 2)*Mathf.Pow(t, 2) - Mathf.Pow(ay, 2)*Mathf.Pow(cVz, 2)*Mathf.Pow(t, 2) - Mathf.Pow(az, 2)*Mathf.Pow(cVz, 2)*Mathf.Pow(t, 2) + 2*ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*
		           (-currCoordT + currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) + Mathf.Pow(ax, 2)*(Mathf.Pow(currCoordT, 2) + Mathf.Pow(currCoordX, 2)*Mathf.Pow(cVx, 2) + Mathf.Pow(currCoordY, 2)*Mathf.Pow(cVy, 2) + 2*currCoordY*currCoordZ*cVy*cVz + 
		           Mathf.Pow(currCoordZ, 2)*Mathf.Pow(cVz, 2) + 2*currCoordY*cVy*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + 2*currCoordZ*cVz*Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t + Mathf.Pow(t, 2) - Mathf.Pow(cVx, 2)*Mathf.Pow(t, 2) - Mathf.Pow(cVy, 2)*Mathf.Pow(t, 2) - Mathf.Pow(cVz, 2)*Mathf.Pow(t, 2) + 
		           2*currCoordX*cVx*(currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t) - 2*currCoordT*(currCoordX*cVx + currCoordY*cVy + currCoordZ*cVz + Mathf.Sqrt(1 - Mathf.Pow(cVx, 2) - Mathf.Pow(cVy, 2) - Mathf.Pow(cVz, 2))*t)))))/
		           (-2*ay*az*(B13 - B23*cVx - B33*cVy - B43*cVz)*(-B14 + B24*cVx + B34*cVy + B44*cVz) + Mathf.Pow(ax, 2)*(-Mathf.Pow(B11, 2) + Mathf.Pow(B12, 2) - Mathf.Pow(B21, 2)*Mathf.Pow(cVx, 2) + Mathf.Pow(B22, 2)*Mathf.Pow(cVx, 2) - 2*B21*B31*cVx*cVy + 2*B22*B32*cVx*cVy - Mathf.Pow(B31, 2)*Mathf.Pow(cVy, 2) + Mathf.Pow(B32, 2)*Mathf.Pow(cVy, 2) - 
		           2*B21*B41*cVx*cVz + 2*B22*B42*cVx*cVz - 2*B31*B41*cVy*cVz + 2*B32*B42*cVy*cVz - Mathf.Pow(B41, 2)*Mathf.Pow(cVz, 2) + Mathf.Pow(B42, 2)*Mathf.Pow(cVz, 2) + 2*B11*(B21*cVx + B31*cVy + B41*cVz) - 2*B12*(B22*cVx + B32*cVy + B42*cVz)) + 
		           Mathf.Pow(ay, 2)*(-Mathf.Pow(B11, 2) + Mathf.Pow(B13, 2) - Mathf.Pow(B21, 2)*Mathf.Pow(cVx, 2) + Mathf.Pow(B23, 2)*Mathf.Pow(cVx, 2) - 2*B21*B31*cVx*cVy + 2*B23*B33*cVx*cVy - Mathf.Pow(B31, 2)*Mathf.Pow(cVy, 2) + Mathf.Pow(B33, 2)*Mathf.Pow(cVy, 2) - 2*B21*B41*cVx*cVz + 2*B23*B43*cVx*cVz - 2*B31*B41*cVy*cVz + 2*B33*B43*cVy*cVz - Mathf.Pow(B41, 2)*Mathf.Pow(cVz, 2) + 
		           Mathf.Pow(B43, 2)*Mathf.Pow(cVz, 2) + 2*B11*(B21*cVx + B31*cVy + B41*cVz) - 2*B13*(B23*cVx + B33*cVy + B43*cVz)) + Mathf.Pow(az, 2)*(-Mathf.Pow(B11, 2) + Mathf.Pow(B14, 2) - Mathf.Pow(B21, 2)*Mathf.Pow(cVx, 2) + Mathf.Pow(B24, 2)*Mathf.Pow(cVx, 2) - 2*B21*B31*cVx*cVy + 2*B24*B34*cVx*cVy - Mathf.Pow(B31, 2)*Mathf.Pow(cVy, 2) + Mathf.Pow(B34, 2)*Mathf.Pow(cVy, 2) - 
		           2*B21*B41*cVx*cVz + 2*B24*B44*cVx*cVz - 2*B31*B41*cVy*cVz + 2*B34*B44*cVy*cVz - Mathf.Pow(B41, 2)*Mathf.Pow(cVz, 2) + Mathf.Pow(B44, 2)*Mathf.Pow(cVz, 2) + 2*B11*(B21*cVx + B31*cVy + B41*cVz) - 2*B14*(B24*cVx + B34*cVy + B44*cVz)) - 
		           2*ax*(B12 - B22*cVx - B32*cVy - B42*cVz)*(ay*(-B13 + B23*cVx + B33*cVy + B43*cVz) + az*(-B14 + B24*cVx + B34*cVy + B44*cVz)));
		*/
		//As these are inverses of get_observer_time() (each valid under certain circumstances), feed the results back into get_observer_time() to find which is valid under these circumstances.
		float obs_time1 = get_observer_time(a, coordinate_velocity, object_to_coordinate_boost, current_event_coordinate, T1);
		float obs_time2 = get_observer_time(a, coordinate_velocity, object_to_coordinate_boost, current_event_coordinate, T2);
		if (Mathf.Abs(observer_time - obs_time2) > Mathf.Abs(observer_time - obs_time1)){
			return T1;
		}else{
			return T2;
		}
	}
}
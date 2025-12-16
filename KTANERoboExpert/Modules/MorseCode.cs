using System.Diagnostics;
using System.Speech.Recognition;
using System.Text;

namespace KTANERoboExpert.Modules;

public class MorseCode : RoboExpertModule
{
    public override string Name => "Morse Code";
    public override string Help => "go dot dash done | repeat | reset | language delta echo";
    private Grammar? _grammar;
    public override Grammar Grammar => _grammar ??= new(new Choices("go" + new GrammarBuilder(new Choices("dot", "dash"), 1, 7) + "done", "repeat", "reset", "language" + new GrammarBuilder(new Choices(_translatedWords.Keys.Select(s => s.Select(c => NATO.ElementAt(c - 'a')).Conjoin()).ToArray()))));
    private string _soFar = string.Empty;

    private Maybe<string> _lang = default;
    private Dictionary<string, int> Words => _translatedWords[_lang.OrElse("en")];

    public override void ProcessCommand(string command)
    {
        if (command == "reset")
        {
            Speak("Reset");
            Reset();
            return;
        }

        if (command.StartsWith("language"))
        {
            string lang = new([.. command[9..].Split(' ').Select(s => s[0])]);
            if (_translatedWords.ContainsKey(lang))
            {
                _lang = lang;
                Speak(command);
            }
            else
                Speak("Pardon?");
            return;
        }

        if (command == "repeat")
            _soFar += "X ";
        else
        {
            if (_soFar.IndexOf('X') is var x && x is not -1)
                _soFar = _soFar[(x + 2)..];

            _soFar +=
                command[3..^5]
                .Split(' ')
                .Select(c => c switch { "dot" => ".", "dash" => "-", _ => throw new UnreachableException() })
                .Conjoin(string.Empty)
                + ' ';
        }

        int val = Match(_soFar);
        while (val is 0)
        {
            _soFar = _soFar[(_soFar.IndexOf(' ') + 1)..];
            val = Match(_soFar);
        }
        Speak(val < 100 ? "Noted" : val.ToString());
        if (val > 100)
        {
            Reset();
            ExitSubmenu();
            Solve();
        }
    }

    private int Match(string str)
    {
        str = str.TrimEnd();
        List<string> possible;
        if (str.Contains('X'))
        {
            var parts = str.Split(' ');
            var x = Array.IndexOf(parts, "X");
            possible = [.. Words.Keys];
            if (x != 0)
            {
                var end = parts[..x].Conjoin();
                possible.RemoveAll(p => !p.EndsWith(end));
            }
            if (x != parts.Length - 1)
            {
                var start = parts[(x + 1)..].Conjoin();
                possible.RemoveAll(p => !p.StartsWith(start));
            }
        }
        else
            possible = [.. Words.Keys.Where(p => p.Contains(str))];

        return possible.Count == 1 ? Words[possible[0]] : possible.Count;
    }

    public override void Cancel() => Reset();
    public override void Reset()
    {
        _soFar = string.Empty;
        _lang = default;
    }

    private static readonly int[] _freqs = [505, 515, 522, 532, 535, 542, 545, 552, 555, 565, 572, 575, 582, 592, 595, 600];

    private static readonly Dictionary<char, string> _letters = new()
    {
        // Latin
        ['a'] = ".-",
        ['b'] = "-...",
        ['c'] = "-.-.",
        ['d'] = "-..",
        ['e'] = ".",
        ['f'] = "..-.",
        ['g'] = "--.",
        ['h'] = "....",
        ['i'] = "..",
        ['j'] = ".---",
        ['k'] = "-.-",
        ['l'] = ".-..",
        ['m'] = "--",
        ['n'] = "-.",
        ['o'] = "---",
        ['p'] = ".--.",
        ['q'] = "--.-",
        ['r'] = ".-.",
        ['s'] = "...",
        ['t'] = "-",
        ['u'] = "..-",
        ['v'] = "...-",
        ['w'] = ".--",
        ['x'] = "-..-",
        ['y'] = "-.--",
        ['z'] = "--..",

        // Latin extended (de, eo, et, fi, fr, no, pl)
        ['ä'] = ".-.-",
        ['ö'] = "---.",
        ['ü'] = "..--",
        ['ß'] = "...--..",
        ['ĉ'] = "-.-..",
        ['ĝ'] = "--.-.",
        ['ĥ'] = "----",
        ['ĵ'] = ".---.",
        ['ŝ'] = "...-.",
        ['ŭ'] = "..--",
        ['å'] = ".--.-",
        ['ê'] = ".",
        ['î'] = "..",
        ['ï'] = "..",
        ['ô'] = "---",
        ['û'] = "..-",
        ['à'] = ".--.-",
        ['ç'] = "-.-..",
        ['é'] = "..-..",
        ['è'] = ".-..-",
        ['ñ'] = "--.--",
        ['œ'] = "--- .",
        ['æ'] = ".-.-",
        ['ø'] = "---.",
        ['ą'] = ".-.-",
        ['ć'] = "-.-..",
        ['ę'] = "..-..",
        ['ł'] = ".-.--",
        ['ń'] = "--.--",
        ['ó'] = "---.",
        ['ś'] = "...-...",
        ['ź'] = "--..-.",
        ['ż'] = "--..-",

        // Korean (ko)
        ['ㄱ'] = ".-..",
        ['\u1100'] = ".-..",
        ['\u11a8'] = ".-..",
        ['ㄴ'] = "..-.",
        ['\u1102'] = "..-.",
        ['\u11ab'] = "..-.",
        ['ㄷ'] = "-...",
        ['\u1103'] = "-...",
        ['\u11ae'] = "-...",
        ['ㄹ'] = "...-",
        ['\u1105'] = "...-",
        ['\u11af'] = "...-",
        ['ㅁ'] = "--",
        ['\u1106'] = "--",
        ['\u11b7'] = "--",
        ['ㅂ'] = ".--",
        ['\u1107'] = ".--",
        ['\u11b8'] = ".--",
        ['ㅅ'] = "--.",
        ['\u1109'] = "--.",
        ['\u11ba'] = "--.",
        ['ㅇ'] = "-.-",
        ['\u110b'] = "-.-",
        ['\u11bc'] = "-.-",
        ['ㅈ'] = ".--.",
        ['\u110c'] = ".--.",
        ['\u11bd'] = ".--.",
        ['ㅊ'] = "-.-.",
        ['\u110e'] = "-.-.",
        ['\u11be'] = "-.-.",
        ['ㅋ'] = "-..-",
        ['\u110f'] = "-..-",
        ['\u11bf'] = "-..-",
        ['ㅌ'] = "--..",
        ['\u1110'] = "--..",
        ['\u11c0'] = "--..",
        ['ㅍ'] = "---",
        ['\u1111'] = "---",
        ['\u11c1'] = "---",
        ['ㅎ'] = ".---",
        ['\u1112'] = ".---",
        ['\u11c2'] = ".---",
        ['ㅏ'] = ".",
        ['\u1161'] = ".",
        ['ㅑ'] = "..",
        ['\u1163'] = "..",
        ['ㅓ'] = "-",
        ['\u1165'] = "-",
        ['ㅕ'] = "...",
        ['\u1167'] = "...",
        ['ㅗ'] = ".-",
        ['\u1169'] = ".-",
        ['ㅛ'] = "-.",
        ['\u116d'] = "-.",
        ['ㅜ'] = "....",
        ['\u116e'] = "....",
        ['ㅠ'] = ".-.",
        ['\u1172'] = ".-.",
        ['ㅡ'] = "-..",
        ['\u1173'] = "-..",
        ['ㅣ'] = "..-",
        ['\u1175'] = "..-",
        ['ㅐ'] = "--.-",
        ['\u1162'] = "--.-",
        ['ㅔ'] = "-.--",
        ['\u1166'] = "-.--",

        // Thai (th)
        ['ก'] = "--.",
        ['ข'] = "-.-.",
        ['ฃ'] = "-.-.",
        ['ค'] = "-.-",
        ['ฅ'] = "-.-",
        ['ฆ'] = "-.-",
        ['ง'] = "-.--.",
        ['จ'] = "-..-.",
        ['ฉ'] = "----",
        ['ช'] = "-..-",
        ['ซ'] = "--..",
        ['ญ'] = ".---",
        ['ด'] = "-..",
        ['ฎ'] = "-..",
        ['ต'] = "-",
        ['ฏ'] = "-",
        ['ถ'] = "-.-..",
        ['ฐ'] = "-.-..",
        ['ท'] = "-..--",
        ['ธ'] = "-..--",
        ['ฑ'] = "-..--",
        ['ฒ'] = "-..--",
        ['น'] = "-.",
        ['ณ'] = "-.",
        ['บ'] = "-...",
        ['ป'] = ".--.",
        ['ผ'] = "--.-",
        ['ฝ'] = "-.-.-",
        ['พ'] = ".--..",
        ['ภ'] = ".--..",
        ['ฟ'] = "..-.",
        ['ม'] = "--",
        ['ย'] = "-.--",
        ['ร'] = ".-.",
        ['ล'] = ".-..",
        ['ฬ'] = ".-..",
        ['ว'] = ".--",
        ['ศ'] = "...",
        ['ษ'] = "...",
        ['ส'] = "...",
        ['ห'] = "....",
        ['อ'] = "-...-",
        ['ฮ'] = "--.--",
        ['ฤ'] = ".-.--",
        ['ๅ'] = "",
        ['ะ'] = ".-...",
        ['า'] = ".-",
        ['ิ'] = "..-..",
        ['ี'] = "..",
        ['ึ'] = "..--.",
        ['ื'] = "..--",
        ['ุ'] = "..-.-",
        ['ู'] = "---.",
        ['เ'] = ".",
        ['แ'] = ".-.-",
        ['ไ'] = ".-..-",
        ['ใ'] = ".-..-",
        ['โ'] = "---",
        ['ำ'] = "...-.",
        ['่'] = "..-",
        ['้'] = "...-",
        ['๊'] = "--...",
        ['๋'] = ".-.-.",
        ['ั'] = ".--.-",
        ['็'] = "---..",
        ['์'] = "--..-",
        ['ๆ'] = "-.---",
        ['ฯ'] = "--.-.",

        // Hebrew (he)
        ['א'] = ".-",
        ['ב'] = "...-",
        ['ג'] = ".--",
        ['ד'] = "..-",
        ['ה'] = "---",
        ['ו'] = ".",
        ['ז'] = "--..",
        ['ח'] = "....",
        ['ט'] = "..-",
        ['י'] = "..",
        ['כ'] = "-.-",
        ['ל'] = ".-..",
        ['מ'] = "--",
        ['ם'] = "--",
        ['נ'] = "-.",
        ['ן'] = "-.",
        ['ס'] = "-.-.",
        ['ע'] = ".---",
        ['פ'] = ".--.",
        ['צ'] = ".--",
        ['ץ'] = ".--",
        ['ק'] = "--.-",
        ['ר'] = ".-.",
        ['ש'] = "...",
        ['ת'] = "-",

        // Cyrillic (ru)
        ['а'] = ".-",
        ['б'] = "-..",
        ['в'] = ".--",
        ['г'] = "--.",
        ['д'] = "-..",
        ['е'] = ".",
        ['ж'] = "...-",
        ['з'] = "--..",
        ['и'] = "..",
        ['й'] = ".---",
        ['к'] = "-.-",
        ['л'] = ".-..",
        ['м'] = "--",
        ['н'] = "....",
        ['о'] = "---",
        ['п'] = ".--.",
        ['р'] = ".-.",
        ['с'] = "...",
        ['т'] = "-",
        ['у'] = "..-",
        ['ф'] = "..-.",
        ['х'] = "....",
        ['ц'] = "-.-.",
        ['ч'] = "---.",
        ['ш'] = "----",
        ['щ'] = "--.-",
        ['ъ'] = "--.--",
        ['ы'] = "-.--",
        ['ь'] = "-..-",
        ['э'] = "..-.",
        ['ю'] = "..--",
        ['я'] = ".-.-",

        // Numbers
        ['0'] = "-----",
        ['1'] = ".----",
        ['2'] = "..---",
        ['3'] = "...--",
        ['4'] = "....-",
        ['5'] = ".....",
        ['6'] = "-....",
        ['7'] = "--...",
        ['8'] = "---..",
        ['9'] = "----.",
    };

    private static readonly Dictionary<string, Dictionary<string, int>> _translatedWords = new()
    {
        ["cs"] = Morsify(["stra ----", "plomba", "bronz", "sklep", "houby", "klouby", "----leba", "pra ----", "shluk", "strop", "bomba", "klenba", "bra ----", "strup", "papou ----", "klacek"]),
        ["da"] = Morsify(["afvis", "blandt", "bomber", "brugt", "firma", "kalder", "klart", "klasse", "klippe", "rekord", "sidst", "skarp", "skifte", "skole", "slippe", "vinder"]),
        ["de"] = Morsify(["hölle", "halle", "hülle", "heiß", "siehe", "si ----el", "lei ----e", "ei ----el", "fis ----er", "si ----er", "si ----t", "sti ----t", "ventil", "steak", "brücke", "rücken"]),
        ["en"] = Morsify(["shell", "halls", "slick", "trick", "boxes", "leaks", "strobe", "bistro", "flick", "bombs", "break", "brick", "steak", "sting", "vector", "beats"]),
        ["eo"] = Morsify(["kontraŭ", "raŭka", "ŝtono", "ĵetono", "risko", "barako", "rompi", "piroj", "butono", "bombo", "batalo", "betono", "radaro", "regulo", "brilas", "pulsi"]),
        ["es"] = Morsify(["biela", "broma", "ratas", "resma", "resta", "brisa", "trenes", "tabla", "senso", "santos", "trato", "braile", "bomba", "tronco", "brinco", "mambo"]),
        ["et"] = Morsify(["parras", "varras", "varas", "emake", "kenake", "nupud", "taimer", "vaikne", "paigal", "puskar", "ääres", "ämblik", "kallis", "juurde", "kuular", "öösel"]),
        ["fi"] = Morsify(["kenttä", "potku", "kettu", "hissi", "katko", "takoi", "rikos", "pirinä", "pentti", "heheh", "pihdit", "koita", "tulos", "pommi", "passi", "lahti"]),
        ["fr"] = Morsify(["coque", "salle", "folie", "piège", "boîte", "fuite", "flash", "resto", "films", "bombe", "casse", "brique", "foudre", "fusée", "bouton", "bruit"]),
        ["he"] = Morsify(["סיסמה", "מהירות", "אורות", "תיאור", "רמזור", "רמאות", "אותיות", "הפלמח", "ההגנה", "עכביש", "ישראל", "אלפים", "קופים", "פיצוץ", "גולני", "גבעתי"]),
        ["it"] = Morsify(["daino", "attico", "antico", "attesa", "esame", "saldi", "salmo", "salame", "soldi", "suono", "tazza", "tassa", "tatto", "zaino", "tuono", "trono"]),
        ["jp"] = Morsify(["shell", "halls", "slick", "trick", "boxes", "leaks", "strobe", "bistro", "flick", "bombs", "break", "brick", "steak", "sting", "vector", "beats"]),
        ["ko"] = Morsify(["김밥", "긴장", "간장", "공장", "궁정", "집밥", "잡담", "적당", "직장", "족장", "안장", "영장", "억장", "인장", "인정"], "ko"),
        ["nl"] = Morsify(["stuur", "buurt", "truus", "uurtje", "trucs", "broek", "bruin", "bruis", "bruist", "ruist", "ruiste", "suist", "suiste", "rijst", "prijs", "prijst"]),
        ["no"] = Morsify(["morse", "økning", "åpning", "signal", "pokal", "sterk", "stopp", "vondt", "banan", "start", "vandre", "andre", "bombe", "varsom", "liksom", "sommer"]),
        ["pl"] = Morsify(["muszla", "świnka", "mamut", "szprot", "skrzynki", "krzesło", "praca", "myszka", "katalog", "bomba", "przerwa", "kostka", "cegła", "mięso", "obszar", "bestia"]),
        ["pt"] = Morsify(["bossa", "bomba", "batom", "bombom", "burro", "dados", "dossiê", "doido", "morse", "módulo", "senha", "serial", "samba", "chave", "código", "conta"]),
        ["ru"] = Morsify(["брать", "брала", "борис", "крала", "клара", "поток", "порог", "порок", "покой", "помой", "попей", "рокер", "койка", "росток", "мостик", "токарь"]),
        ["sv"] = Morsify(["skala", "kalas", "blick", "stick", "dimma", "dekal", "ordlek", "rekord", "klick", "dejta", "stark", "drick", "steka", "stiga", "riktig", "bleka"]),
        ["th"] = Morsify(["ระเบิด", "ระเหิด", "ไข่ไก่", "ไก่ไข่", "บริการ", "บริบาล", "บริหาร", "บริบท", "สะสาง", "เพิ่มพูน", "ภาคภูมิ", "โอกาส", "อากาศ", "สัญญา", "สัญญาณ", "สงสาร"]),
        ["zhs"] = Morsify(["shell", "halls", "slick", "trick", "boxes", "leaks", "strobe", "bistro", "flick", "bombs", "break", "brick", "steak", "sting", "vector", "beats"])
    };

    private static Dictionary<string, int> Morsify(string[] words, Maybe<string> lang = default) => words.Select((e, i) => (e, i)).ToDictionary(w => Morsify(w.e, lang), w => _freqs[w.i]);

    public static string Morsify(string w, Maybe<string> lang = default)
    {
        if (!lang.Exists)
            w = w.ToLowerInvariant();

        if (lang.Exists && lang.Item == "cs" || lang.Item == "de" || lang.Item == "pl")
            if (w.Contains("ch"))
                throw new NotImplementedException("The 'ch' digraph in the requested language is unsupported.");

        if (lang.Exists && lang.Item == "ko")
            w = w.Normalize(NormalizationForm.FormD);

        StringBuilder res = new();
        foreach (char c in w)
        {
            if (c is ' ' or '.' or '-')
                res.Append(c);
            else
            {
                if (res.Length is not 0)
                    res.Append(' ');
                if (lang.Exists && lang.Item == "no" && c == 'è')
                    res.Append("..-..");
                else
                    res.Append(_letters[c]);
            }
        }
        return res.ToString();
    }
}

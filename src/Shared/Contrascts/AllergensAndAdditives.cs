[Flags]
public enum AllergensAndAdditives : ulong
{
    None = 0,

    GlutenhaltigesGetreide = 1ul << 0,
    Krebstiere_und_Krebstiererzeugnisse = 1ul << 1,
    Eier_und_Eierzeugnisse = 1ul << 2,
    Fisch_und_Fischerzeugnisse = 1ul << 3,
    Erdnüsse_und_Erdnusserzeugnisse = 1ul << 4,
    Soja_und_Sojaerzeugnisse = 1ul << 5,
    Milch_und_Milcherzeugnisse = 1ul << 6,
    Schalenfrüchte = 1ul << 7,
    Sellerie_und_Sellerieerzeugnisse = 1ul << 8,
    Senf_und_Senferzeugnisse = 1ul << 9,
    Sesamsamen_und_Sesamsamenerzeugnisse = 1ul << 10,
    Schwefeldioxid_und_Sulfite = 1ul << 11,
    Lupine_und_Lupinenerzeugnisse = 1ul << 12,
    Weichtiere_Mollusken = 1ul << 13,

    Lebensmittelfarbe = 1ul << 14,
    Konservierungsstoffe = 1ul << 15,
    Antioxidationsmittel = 1ul << 16,
    Geschmacksverstärker = 1ul << 17,
    Geschwefelt = 1ul << 18,
    Geschwärzt = 1ul << 19,
    Gewachst = 1ul << 20,
    Phosphat = 1ul << 21,
    Süßungsmittel = 1ul << 22,
    Phenylalaninquelle = 1ul << 23,

    Weizen = 1ul << 24,
    Dinkel = 1ul << 25,
    Roggen = 1ul << 26,
    Gerste = 1ul << 27,
    Hafer = 1ul << 28,

    Mandeln = 1ul << 29,
    Haselnüsse = 1ul << 30,
    Walnüsse = 1ul << 31,
    Cashewnüsse = 1ul << 32,
    Pekannüsse = 1ul << 33,
    Paranüsse = 1ul << 34,
    Pistazien = 1ul << 35,
    Macadamianüsse = 1ul << 36,
}

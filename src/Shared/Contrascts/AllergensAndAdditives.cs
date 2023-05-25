[Flags]
public enum AllergensAndAdditives
{
    None = 0,

    GlutenhaltigesGetreide = 1 << 0,
    Krebstiere_und_Krebstiererzeugnisse = 1 << 1,
    Eier_und_Eierzeugnisse = 1 << 2,
    Fisch_und_Fischerzeugnisse = 1 << 3,
    Erdnüsse_und_Erdnusserzeugnisse = 1 << 4,
    Soja_und_Sojaerzeugnisse = 1 << 5,
    Milch_und_Milcherzeugnisse = 1 << 6,
    Schalenfrüchte = 1 << 7,
    Sellerie_und_Sellerieerzeugnisse = 1 << 8,
    Senf_und_Senferzeugnisse = 1 << 9,
    Sesamsamen_und_Sesamsamenerzeugnisse = 1 << 10,
    Schwefeldioxid_und_Sulfite = 1 << 11,
    Lupine_und_Lupinenerzeugnisse = 1 << 12,
    Weichtiere_Mollusken = 1 << 13,

    Lebensmittelfarbe = 1 << 14,
    Konservierungsstoffe = 1 << 15,
    Antioxidationsmittel = 1 << 16,
    Geschmacksverstärker = 1 << 17,
    Geschwefelt = 1 << 18,
    Geschwärzt = 1 << 19,
    Gewachst = 1 << 20,
    Phosphat = 1 << 21,
    Süßungsmittel = 1 << 22,
    Phenylalaninquelle = 1 << 23,
}

using System;
using System.IO;
using System.Text;
using WebCore.Utils;

namespace WebCore.Compression
{
    /// <summary>
    /// Compression for very small strings
    /// </summary>
    public static class SmallString
    {
        #region Lookup Data
        private static readonly byte[][] encoderLookup = {
            new byte[] {2, 115, 44, 182}, new byte[] {3, 104, 97, 100, 154, 2, 108, 101, 87}, new byte[] {3, 111, 110, 32, 142}, null, new byte[] {1, 121, 83},
            new byte[] {2, 109, 97, 173, 2, 108, 105, 151}, new byte[] {3, 111, 114, 32, 176}, null, new byte[] {2, 108, 108, 152, 3, 115, 32, 116, 191},
            new byte[] {4, 102, 114, 111, 109, 103, 2, 109, 101, 108}, null, new byte[] {3, 105, 116, 115, 218}, new byte[] {1, 122, 219},
            new byte[] {3, 105, 110, 103, 70}, new byte[] {1, 62, 222}, new byte[] {1, 32, 0, 3, 32, 32, 32, 40, 2, 110, 99, 228},
            new byte[] {2, 110, 100, 61, 3, 32, 111, 110, 202}, new byte[] {2, 110, 101, 139, 3, 104, 97, 116, 190, 3, 114, 101, 32, 113}, null,
            new byte[] {2, 110, 103, 84, 3, 104, 101, 114, 122, 4, 104, 97, 118, 101, 198, 3, 115, 32, 111, 149}, null,
            new byte[] {3, 105, 111, 110, 107, 3, 115, 32, 97, 172, 2, 108, 121, 234}, new byte[] {3, 104, 105, 115, 76, 3, 32, 105, 110, 78, 3, 32, 98, 101, 170},
            null, new byte[] {3, 32, 102, 111, 213, 3, 32, 111, 102, 32, 3, 32, 104, 97, 201}, null, new byte[] {2, 111, 102, 5},
            new byte[] {3, 32, 99, 111, 161, 2, 110, 111, 183, 3, 32, 109, 97, 248}, null, null,
            new byte[] {3, 32, 99, 108, 238, 3, 101, 110, 116, 97, 3, 32, 97, 110, 55}, new byte[] {2, 110, 115, 192, 1, 34, 101},
            new byte[] {3, 110, 32, 116, 143, 2, 110, 116, 80, 3, 115, 44, 32, 133}, new byte[] {2, 112, 101, 208, 3, 32, 119, 101, 233, 2, 111, 109, 147},
            new byte[] {2, 111, 110, 31}, null, new byte[] {2, 121, 32, 71}, new byte[] {3, 32, 119, 97, 185}, new byte[] {3, 32, 114, 101, 209, 2, 111, 114, 42},
            null, new byte[] {2, 61, 34, 169, 2, 111, 116, 223}, new byte[] {3, 102, 111, 114, 68, 2, 111, 117, 91}, new byte[] {3, 32, 116, 111, 82},
            new byte[] {3, 32, 116, 104, 13}, new byte[] {3, 32, 105, 116, 246}, new byte[] {3, 98, 117, 116, 177, 2, 114, 97, 130, 3, 32, 119, 105, 243, 2, 60, 47, 241},
            new byte[] {3, 32, 119, 104, 159}, new byte[] {2, 32, 32, 52}, new byte[] {3, 110, 100, 32, 63}, new byte[] {2, 114, 101, 33}, null,
            new byte[] {3, 110, 103, 32, 99}, null, new byte[] {3, 108, 121, 32, 199, 3, 97, 115, 115, 211, 1, 97, 4, 2, 114, 105, 114}, null, null, null,
            new byte[] {2, 115, 101, 95}, new byte[] {3, 111, 102, 32, 34}, new byte[] {3, 100, 105, 118, 244, 2, 114, 111, 115, 3, 101, 114, 101, 160}, null,
            new byte[] {2, 116, 97, 200, 1, 98, 90, 2, 115, 105, 212}, null, new byte[] {3, 97, 110, 100, 7, 2, 114, 115, 221}, new byte[] {2, 114, 116, 242},
            new byte[] {2, 116, 101, 69}, new byte[] {3, 97, 116, 105, 206}, new byte[] {2, 115, 111, 179}, new byte[] {2, 116, 104, 17},
            new byte[] {2, 116, 105, 74, 1, 99, 28, 3, 97, 108, 108, 112}, new byte[] {3, 97, 116, 101, 229}, new byte[] {2, 115, 115, 166}, new byte[] {2, 115, 116, 77},
            null, new byte[] {2, 62, 60, 230}, new byte[] {2, 116, 111, 20}, new byte[] {3, 97, 114, 101, 119}, new byte[] {1, 100, 24}, new byte[] {2, 116, 114, 195},
            null, new byte[] {1, 10, 49, 3, 32, 97, 32, 146}, new byte[] {3, 102, 32, 116, 118, 2, 118, 101, 111}, new byte[] {2, 117, 110, 224}, null,
            new byte[] {3, 101, 32, 111, 162}, new byte[] {2, 97, 32, 163, 2, 119, 97, 214, 1, 101, 2}, new byte[] {2, 117, 114, 150, 3, 101, 32, 97, 188},
            new byte[] {2, 117, 115, 164, 3, 10, 13, 10, 167}, new byte[] {2, 117, 116, 196, 3, 101, 32, 99, 251}, new byte[] {2, 119, 101, 145}, null, null,
            new byte[] {2, 119, 104, 194}, new byte[] {1, 102, 44}, null, null, null, new byte[] {3, 100, 32, 116, 134}, null, null, new byte[] {3, 116, 104, 32, 227},
            new byte[] {1, 103, 59}, null, null, new byte[] {1, 13, 57, 3, 101, 32, 115, 181}, new byte[] {3, 101, 32, 116, 156}, null, new byte[] {3, 116, 111, 32, 89},
            new byte[] {3, 101, 13, 10, 158}, new byte[] {2, 100, 32, 30, 1, 104, 18}, null, new byte[] {1, 44, 81}, new byte[] {2, 32, 97, 25},
            new byte[] {2, 32, 98, 94}, new byte[] {2, 13, 10, 21, 2, 32, 99, 73}, new byte[] {2, 32, 100, 165}, new byte[] {2, 32, 101, 171},
            new byte[] {2, 32, 102, 104, 1, 105, 8, 2, 101, 32, 11}, null, new byte[] {2, 32, 104, 85, 1, 45, 204}, new byte[] {2, 32, 105, 56}, null, null,
            new byte[] {2, 32, 108, 205}, new byte[] {2, 32, 109, 123}, new byte[] {2, 102, 32, 58, 2, 32, 110, 236}, new byte[] {2, 32, 111, 29},
            new byte[] {2, 32, 112, 125, 1, 46, 110, 3, 13, 10, 13, 168}, null, new byte[] {2, 32, 114, 189}, new byte[] {2, 32, 115, 62}, new byte[] {2, 32, 116, 14},
            null, new byte[] {2, 103, 32, 157, 5, 119, 104, 105, 99, 104, 43, 3, 119, 104, 105, 247}, new byte[] {2, 32, 119, 53}, new byte[] {1, 47, 197},
            new byte[] {3, 97, 115, 32, 140}, new byte[] {3, 97, 116, 32, 135}, null, new byte[] {3, 119, 104, 111, 217}, null, new byte[] {1, 108, 22, 2, 104, 32, 138},
            null, new byte[] {2, 44, 32, 36}, null, new byte[] {4, 119, 105, 116, 104, 86}, null, null, null, new byte[] {1, 109, 45}, null, null,
            new byte[] {2, 97, 99, 239}, new byte[] {2, 97, 100, 232}, new byte[] {3, 84, 104, 101, 72}, null, null, new byte[] {4, 116, 104, 105, 115, 155, 1, 110, 9},
            null, new byte[] {2, 46, 32, 121}, null, new byte[] {2, 97, 108, 88, 3, 101, 44, 32, 245}, new byte[] {3, 116, 105, 111, 141, 2, 98, 101, 92},
            new byte[] {2, 97, 110, 26, 3, 118, 101, 114, 231}, null, new byte[] {4, 116, 104, 97, 116, 48, 3, 116, 104, 97, 203, 1, 111, 6},
            new byte[] {3, 119, 97, 115, 50}, new byte[] {2, 97, 114, 79}, new byte[] {2, 97, 115, 46},
            new byte[] {2, 97, 116, 39, 3, 116, 104, 101, 1, 4, 116, 104, 101, 121, 128, 5, 116, 104, 101, 114, 101, 210, 5, 116, 104, 101, 105, 114, 100},
            new byte[] {2, 99, 101, 136}, new byte[] {4, 119, 101, 114, 101, 93}, null, new byte[] {2, 99, 104, 153, 2, 108, 32, 180, 1, 112, 60}, null, null,
            new byte[] {3, 111, 110, 101, 174}, null, new byte[] {3, 104, 101, 32, 19, 2, 100, 101, 106}, new byte[] {3, 116, 101, 114, 184},
            new byte[] {2, 99, 111, 117}, null, new byte[] {2, 98, 121, 127, 2, 100, 105, 129, 2, 101, 97, 120}, null, new byte[] {2, 101, 99, 215},
            new byte[] {2, 101, 100, 66}, new byte[] {2, 101, 101, 235}, null, null, new byte[] {1, 114, 12, 2, 110, 32, 41}, null, null, null,
            new byte[] {2, 101, 108, 178}, null, new byte[] {3, 105, 110, 32, 105, 2, 101, 110, 51}, null, new byte[] {2, 111, 32, 96, 1, 115, 10}, null,
            new byte[] {2, 101, 114, 27}, new byte[] {3, 105, 115, 32, 116, 2, 101, 115, 54}, null, new byte[] {2, 103, 101, 249}, new byte[] {4, 46, 99, 111, 109, 253},
            new byte[] {2, 102, 111, 220, 3, 111, 117, 114, 216}, new byte[] {3, 99, 104, 32, 193, 1, 116, 3}, new byte[] {2, 104, 97, 98}, null,
            new byte[] {3, 109, 101, 110, 252}, null, new byte[] {2, 104, 101, 16}, null, null, new byte[] {1, 117, 38}, new byte[] {2, 104, 105, 102}, null,
            new byte[] {3, 110, 111, 116, 132, 2, 105, 99, 131}, new byte[] {3, 101, 100, 32, 64, 2, 105, 100, 237}, null, null, new byte[] {2, 104, 111, 187},
            new byte[] {2, 114, 32, 75, 1, 118, 109}, null, null, null, new byte[] {3, 116, 32, 116, 175, 2, 105, 108, 240}, new byte[] {2, 105, 109, 226},
            new byte[] {3, 101, 110, 32, 207, 2, 105, 110, 15}, new byte[] {2, 105, 111, 144}, new byte[] {2, 115, 32, 23, 1, 119, 65}, null,
            new byte[] {3, 101, 114, 32, 124}, new byte[] {3, 101, 115, 32, 126, 2, 105, 115, 37}, new byte[] {2, 105, 116, 47}, null, new byte[] {2, 105, 118, 186},
            null, new byte[] {2, 116, 32, 35, 7, 104, 116, 116, 112, 58, 47, 47, 67, 1, 120, 250}, new byte[] {2, 108, 97, 137}, new byte[] {1, 60, 225},
            new byte[] {3, 44, 32, 97, 148}
        };

        /* Decode List
        000 = <space>                 051 = en                      102 = hi                      153 = ch                      204 = -                       
        001 = the                     052 = <space><space>          103 = from                    154 = had                     205 = <space>l                
        002 = e                       053 = <space>w                104 = <space>f                155 = this                    206 = ati                     
        003 = t                       054 = es                      105 = in<space>               156 = e<space>t               207 = en<space>               
        004 = a                       055 = <space>an               106 = de                      157 = g<space>                208 = pe                      
        005 = of                      056 = <space>i                107 = ion                     158 = e<cr><lf>               209 = <space>re               
        006 = o                       057 = <cr>                    108 = me                      159 = <space>wh               210 = there                   
        007 = and                     058 = f<space>                109 = v                       160 = ere                     211 = ass                     
        008 = i                       059 = g                       110 = .                       161 = <space>co               212 = si                      
        009 = n                       060 = p                       111 = ve                      162 = e<space>o               213 = <space>fo               
        010 = s                       061 = nd                      112 = all                     163 = a<space>                214 = wa                      
        011 = e<space>                062 = <space>s                113 = re<space>               164 = us                      215 = ec                      
        012 = r                       063 = nd<space>               114 = ri                      165 = <space>d                216 = our                     
        013 = <space>th               064 = ed<space>               115 = ro                      166 = ss                      217 = who                     
        014 = <space>t                065 = w                       116 = is<space>               167 = <lf><cr><lf>            218 = its                     
        015 = in                      066 = ed                      117 = co                      168 = <cr><lf><cr>            219 = z                       
        016 = he                      067 = http://                 118 = f<space>t               169 = ="                      220 = fo                      
        017 = th                      068 = for                     119 = are                     170 = <space>be               221 = rs                      
        018 = h                       069 = te                      120 = ea                      171 = <space>e                222 = >                       
        019 = he<space>               070 = ing                     121 = .<space>                172 = s<space>a               223 = ot                      
        020 = to                      071 = y<space>                122 = her                     173 = ma                      224 = un                      
        021 = <cr><lf>                072 = The                     123 = <space>m                174 = one                     225 = <                       
        022 = l                       073 = <space>c                124 = er<space>               175 = t<space>t               226 = im                      
        023 = s<space>                074 = ti                      125 = <space>p                176 = or<space>               227 = th<space>               
        024 = d                       075 = r<space>                126 = es<space>               177 = but                     228 = nc                      
        025 = <space>a                076 = his                     127 = by                      178 = el                      229 = ate                     
        026 = an                      077 = st                      128 = they                    179 = so                      230 = ><                      
        027 = er                      078 = <space>in               129 = di                      180 = l<space>                231 = ver                     
        028 = c                       079 = ar                      130 = ra                      181 = e<space>s               232 = ad                      
        029 = <space>o                080 = nt                      131 = ic                      182 = s,                      233 = <space>we               
        030 = d<space>                081 = ,                       132 = not                     183 = no                      234 = ly                      
        031 = on                      082 = <space>to               133 = s,<space>               184 = ter                     235 = ee                      
        032 = <space>of               083 = y                       134 = d<space>t               185 = <space>wa               236 = <space>n                
        033 = re                      084 = ng                      135 = at<space>               186 = iv                      237 = id                      
        034 = of<space>               085 = <space>h                136 = ce                      187 = ho                      238 = <space>cl               
        035 = t<space>                086 = with                    137 = la                      188 = e<space>a               239 = ac                      
        036 = ,<space>                087 = le                      138 = h<space>                189 = <space>r                240 = il                      
        037 = is                      088 = al                      139 = ne                      190 = hat                     241 = </                      
        038 = u                       089 = to<space>               140 = as<space>               191 = s<space>t               242 = rt                      
        039 = at                      090 = b                       141 = tio                     192 = ns                      243 = <space>wi               
        040 = <space><space><space>   091 = ou                      142 = on<space>               193 = ch<space>               244 = div                     
        041 = n<space>                092 = be                      143 = n<space>t               194 = wh                      245 = e,<space>               
        042 = or                      093 = were                    144 = io                      195 = tr                      246 = <space>it               
        043 = which                   094 = <space>b                145 = we                      196 = ut                      247 = whi                     
        044 = f                       095 = se                      146 = <space>a<space>         197 = /                       248 = <space>ma               
        045 = m                       096 = o<space>                147 = om                      198 = have                    249 = ge                      
        046 = as                      097 = ent                     148 = ,<space>a               199 = ly<space>               250 = x                       
        047 = it                      098 = ha                      149 = s<space>o               200 = ta                      251 = e<space>c               
        048 = that                    099 = ng<space>               150 = ur                      201 = <space>ha               252 = men                     
        049 = <lf>                    100 = their                   151 = li                      202 = <space>on               253 = .com                    
        050 = was                     101 = "                       152 = ll                      203 = tha                     
        */
        private static readonly byte[][] decoderLookup =
        {
            new byte[] {32}, new byte[] {116, 104, 101}, new byte[] {101}, new byte[] {116}, new byte[] {97}, new byte[] {111, 102}, new byte[] {111},
            new byte[] {97, 110, 100}, new byte[] {105}, new byte[] {110}, new byte[] {115}, new byte[] {101, 32}, new byte[] {114},
            new byte[] {32, 116, 104}, new byte[] {32, 116}, new byte[] {105, 110}, new byte[] {104, 101}, new byte[] {116, 104}, new byte[] {104},
            new byte[] {104, 101, 32}, new byte[] {116, 111}, new byte[] {13, 10}, new byte[] {108}, new byte[] {115, 32}, new byte[] {100},
            new byte[] {32, 97}, new byte[] {97, 110}, new byte[] {101, 114}, new byte[] {99}, new byte[] {32, 111}, new byte[] {100, 32},
            new byte[] {111, 110}, new byte[] {32, 111, 102}, new byte[] {114, 101}, new byte[] {111, 102, 32}, new byte[] {116, 32},
            new byte[] {44, 32}, new byte[] {105, 115}, new byte[] {117}, new byte[] {97, 116}, new byte[] {32, 32, 32}, new byte[] {110, 32},
            new byte[] {111, 114}, new byte[] {119, 104, 105, 99, 104}, new byte[] {102}, new byte[] {109}, new byte[] {97, 115},
            new byte[] {105, 116}, new byte[] {116, 104, 97, 116}, new byte[] {10}, new byte[] {119, 97, 115}, new byte[] {101, 110},
            new byte[] {32, 32}, new byte[] {32, 119}, new byte[] {101, 115}, new byte[] {32, 97, 110}, new byte[] {32, 105}, new byte[] {13},
            new byte[] {102, 32}, new byte[] {103}, new byte[] {112}, new byte[] {110, 100}, new byte[] {32, 115}, new byte[] {110, 100, 32},
            new byte[] {101, 100, 32}, new byte[] {119}, new byte[] {101, 100}, new byte[] {104, 116, 116, 112, 58, 47, 47}, new byte[] {102, 111, 114},
            new byte[] {116, 101}, new byte[] {105, 110, 103}, new byte[] {121, 32}, new byte[] {84, 104, 101}, new byte[] {32, 99},
            new byte[] {116, 105}, new byte[] {114, 32}, new byte[] {104, 105, 115}, new byte[] {115, 116}, new byte[] {32, 105, 110},
            new byte[] {97, 114}, new byte[] {110, 116}, new byte[] {44}, new byte[] {32, 116, 111}, new byte[] {121}, new byte[] {110, 103},
            new byte[] {32, 104}, new byte[] {119, 105, 116, 104}, new byte[] {108, 101}, new byte[] {97, 108}, new byte[] {116, 111, 32},
            new byte[] {98}, new byte[] {111, 117}, new byte[] {98, 101}, new byte[] {119, 101, 114, 101}, new byte[] {32, 98},
            new byte[] {115, 101}, new byte[] {111, 32}, new byte[] {101, 110, 116}, new byte[] {104, 97}, new byte[] {110, 103, 32},
            new byte[] {116, 104, 101, 105, 114}, new byte[] {34}, new byte[] {104, 105}, new byte[] {102, 114, 111, 109}, new byte[] {32, 102},
            new byte[] {105, 110, 32}, new byte[] {100, 101}, new byte[] {105, 111, 110}, new byte[] {109, 101}, new byte[] {118}, new byte[] {46},
            new byte[] {118, 101}, new byte[] {97, 108, 108}, new byte[] {114, 101, 32}, new byte[] {114, 105}, new byte[] {114, 111},
            new byte[] {105, 115, 32}, new byte[] {99, 111}, new byte[] {102, 32, 116}, new byte[] {97, 114, 101}, new byte[] {101, 97},
            new byte[] {46, 32}, new byte[] {104, 101, 114}, new byte[] {32, 109}, new byte[] {101, 114, 32}, new byte[] {32, 112},
            new byte[] {101, 115, 32}, new byte[] {98, 121}, new byte[] {116, 104, 101, 121}, new byte[] {100, 105}, new byte[] {114, 97},
            new byte[] {105, 99}, new byte[] {110, 111, 116}, new byte[] {115, 44, 32}, new byte[] {100, 32, 116}, new byte[] {97, 116, 32},
            new byte[] {99, 101}, new byte[] {108, 97}, new byte[] {104, 32}, new byte[] {110, 101}, new byte[] {97, 115, 32}, new byte[] {116, 105, 111},
            new byte[] {111, 110, 32}, new byte[] {110, 32, 116}, new byte[] {105, 111}, new byte[] {119, 101}, new byte[] {32, 97, 32},
            new byte[] {111, 109}, new byte[] {44, 32, 97}, new byte[] {115, 32, 111}, new byte[] {117, 114}, new byte[] {108, 105},
            new byte[] {108, 108}, new byte[] {99, 104}, new byte[] {104, 97, 100}, new byte[] {116, 104, 105, 115}, new byte[] {101, 32, 116},
            new byte[] {103, 32}, new byte[] {101, 13, 10}, new byte[] {32, 119, 104}, new byte[] {101, 114, 101}, new byte[] {32, 99, 111},
            new byte[] {101, 32, 111}, new byte[] {97, 32}, new byte[] {117, 115}, new byte[] {32, 100}, new byte[] {115, 115}, new byte[] {10, 13, 10},
            new byte[] {13, 10, 13}, new byte[] {61, 34}, new byte[] {32, 98, 101}, new byte[] {32, 101}, new byte[] {115, 32, 97}, new byte[] {109, 97},
            new byte[] {111, 110, 101}, new byte[] {116, 32, 116}, new byte[] {111, 114, 32}, new byte[] {98, 117, 116}, new byte[] {101, 108},
            new byte[] {115, 111}, new byte[] {108, 32}, new byte[] {101, 32, 115}, new byte[] {115, 44}, new byte[] {110, 111}, new byte[] {116, 101, 114},
            new byte[] {32, 119, 97}, new byte[] {105, 118}, new byte[] {104, 111}, new byte[] {101, 32, 97}, new byte[] {32, 114}, new byte[] {104, 97, 116},
            new byte[] {115, 32, 116}, new byte[] {110, 115}, new byte[] {99, 104, 32}, new byte[] {119, 104}, new byte[] {116, 114}, new byte[] {117, 116},
            new byte[] {47}, new byte[] {104, 97, 118, 101}, new byte[] {108, 121, 32}, new byte[] {116, 97}, new byte[] {32, 104, 97}, new byte[] {32, 111, 110},
            new byte[] {116, 104, 97}, new byte[] {45}, new byte[] {32, 108}, new byte[] {97, 116, 105}, new byte[] {101, 110, 32}, new byte[] {112, 101},
            new byte[] {32, 114, 101}, new byte[] {116, 104, 101, 114, 101}, new byte[] {97, 115, 115}, new byte[] {115, 105}, new byte[] {32, 102, 111},
            new byte[] {119, 97}, new byte[] {101, 99}, new byte[] {111, 117, 114}, new byte[] {119, 104, 111}, new byte[] {105, 116, 115}, new byte[] {122},
            new byte[] {102, 111}, new byte[] {114, 115}, new byte[] {62}, new byte[] {111, 116}, new byte[] {117, 110}, new byte[] {60},
            new byte[] {105, 109}, new byte[] {116, 104, 32}, new byte[] {110, 99}, new byte[] {97, 116, 101}, new byte[] {62, 60},
            new byte[] {118, 101, 114}, new byte[] {97, 100}, new byte[] {32, 119, 101}, new byte[] {108, 121}, new byte[] {101, 101}, new byte[] {32, 110},
            new byte[] {105, 100}, new byte[] {32, 99, 108}, new byte[] {97, 99}, new byte[] {105, 108}, new byte[] {60, 47}, new byte[] {114, 116},
            new byte[] {32, 119, 105}, new byte[] {100, 105, 118}, new byte[] {101, 44, 32}, new byte[] {32, 105, 116}, new byte[] {119, 104, 105},
            new byte[] {32, 109, 97}, new byte[] {103, 101}, new byte[] {120}, new byte[] {101, 32, 99}, new byte[] {109, 101, 110}, new byte[] {46, 99, 111, 109}
        };
        #endregion

        /// <summary>
        /// Compresses a byte array using Smaz encoding
        /// </summary>
        /// <param name="Input">ASCII encoded Data to be compressed</param>
        /// <returns>Compressed data as a byte array</returns>
        public static byte[] Compress(byte[] Input)
        {
            // Output buffer
            byte[] buf = new byte[Input.Length];
            int bufPos = 0;

            int inLen = Input.Length;

            int inPos = 0,
                verbPos = 0;
            byte verbLen = 0;

            int h1, h2, h3 = 0;

            // Loop through each input character
            while (inLen > 0)
            {
                int j = 7;
                bool found = false;

                // Determine encoder lookup index
                h1 = h2 = Input[inPos] << 3;
                if (inLen > 1)
                {
                    h2 += Input[inPos + 1];
                }
                if (inLen > 2)
                {
                    h3 = h2 ^ Input[inPos + 2];
                }
                if (j > inLen)
                {
                    j = inLen;
                }

                // Search for encoder words (from largest (7) to smallest (1))
                for (; j > 0; j--)
                {
                    // Fetch relevant encoder slot (based on word length (j) and input data (h1, h2, h3))
                    byte[] slot;
                    switch (j)
                    {
                        case 1:
                            slot = encoderLookup[h1 % 241];
                            break;
                        case 2:
                            slot = encoderLookup[h2 % 241];
                            break;
                        default:
                            slot = encoderLookup[h3 % 241];
                            break;
                    }

                    // Skip if slot is null
                    if (slot != null)
                    {
                        // Iterate through all slot word entries
                        var slotPos = 0;
                        while (slot.Length > slotPos)
                        {
                            // Check slot word length's match
                            if (slot[slotPos] == j)
                            {
                                // Byte-wise compare slot word to input
                                found = true;
                                for (int i = 0; i < j; i++)
                                {
                                    if (Input[inPos + i] != slot[slotPos + i + 1])
                                    {
                                        // Doesn't match, break and try next slot word
                                        found = false;
                                        break;
                                    }
                                }

                                if (found)
                                {
                                    // Check if output buffer resize is required
                                    if (bufPos + verbLen + 3 > buf.Length)
                                    {
                                        Array.Resize(ref buf, buf.Length * 2);
                                    }

                                    // Write verbatim data (where no encoder word is available)
                                    if (verbLen > 0)
                                    {
                                        if (verbLen == 1)
                                        {
                                            // Single byte, prefix with [254]
                                            buf[bufPos++] = 254;
                                            buf[bufPos++] = Input[verbPos];
                                        }
                                        else
                                        {
                                            // Multiple bytes, prefix with [255] and [length]
                                            buf[bufPos++] = 255;
                                            buf[bufPos++] = verbLen;
                                            Array.Copy(Input, Input.Length - inLen - verbLen, buf, bufPos, verbLen);
                                            bufPos += verbLen;
                                        }
                                    }

                                    // Write encoded word index
                                    buf[bufPos++] = slot[slot[slotPos] + slotPos + 1];

                                    // Move input position forward (by the length of the encoded word)
                                    inLen -= j;
                                    inPos += j;

                                    // Reset verbatim position (to the current input position)
                                    verbPos = inPos;
                                    verbLen = 0;

                                    // Found word, so break slot loop, and continue at new input position
                                    break;
                                }
                            }

                            // Try next word in slot
                            slotPos += slot[slotPos] + 2;
                        }

                        if (found)
                        {
                            // Word found, so break word-length loop, and continue at new input position
                            break;
                        }
                    }
                }

                if (!found)
                {
                    // No encoder word was found, so increase verbatim length,
                    //   and continue at the new input position
                    verbLen++;
                    inLen--;
                    inPos++;
                }

                // If verbatim length is maximum (255), or if the end is reached
                //   then flush the verbatim data to the output buffer
                if (verbLen == 255 || (verbLen > 0 && inLen == 0))
                {
                    // Check if output buffer resize is required
                    if (bufPos + verbLen + 2 > buf.Length)
                    {
                        Array.Resize(ref buf, buf.Length * 2);
                    }

                    // Write verbatim data to the output buffer
                    if (verbLen == 1)
                    {
                        // Single byte, prefix with [254]
                        buf[bufPos++] = 254;
                        buf[bufPos++] = Input[verbPos];
                    }
                    else
                    {
                        // Multiple bytes, prefix with [255] and [length]
                        buf[bufPos++] = 255;
                        buf[bufPos++] = verbLen;
                        Array.Copy(Input, Input.Length - inLen - verbLen, buf, bufPos, verbLen);
                        bufPos += verbLen;
                    }

                    // Reset verbatim position (to the current input position)
                    verbPos = inPos;
                    verbLen = 0;
                }
            }

            // Size output buffer to final size
            if (bufPos != buf.Length)
            {
                Array.Resize(ref buf, bufPos);
            }

            // Return output buffer
            return buf;
        }

        /// <summary>
        /// Compresses an ASCII string to a Smaz encoded byte array
        /// </summary>
        /// <param name="Input">String to compress</param>
        /// <returns>Compressed data as a byte array</returns>
        public static byte[] Compress(string Input)
        {
            return Compress(Encoding.ASCII.GetBytes(Input));
        }

        /// <summary>
        /// Decompresses a Smaz encoded byte array and returns the original string
        /// </summary>
        /// <param name="Input">Smaz encoded byte array</param>
        /// <returns>Decompressed string</returns>
        public static string Decompress(byte[] Input)
        {
            // Instantiate an output buffer
            using (MemoryStream output = new MemoryStream(Input.Length * 3))
            {
                for (int i = 0; i < Input.Length; i++)
                {
                    switch (Input[i])
                    {
                        case 254:
                            // Verbatim Byte
                            output.WriteByte(Input[i + 1]);
                            i++;
                            break;
                        case 255:
                            // Verbatim Bytes
                            output.Write(Input, i + 2, Input[i + 1]);
                            i += Input[i + 1] + 1;
                            break;
                        default:
                            // Write Encoded Byte
                            var w = decoderLookup[Input[i]];
                            output.Write(w, 0, w.Length);
                            break;
                    }
                }
                return Encoding.ASCII.GetString(output.ToArray());
            }
        }
    }

    /// <summary>
    /// Based on https://github.com/antirez/smaz
    /// https://ayende.com/blog/172865/reverse-engineering-the-smaz-compression-library
    /// </summary>
    public unsafe class Smaz
    {
        private static readonly string[] DefaultTermsTable = {
            " ", "the", "e", "t", "a", "of", "o", "and", "i", "n", "s", "e ", "r", " th",
            " t", "in", "he", "th", "h", "he ", "to", "\r\n", "l", "s ", "d", " a", "an",
            "er", "c", " o", "d ", "on", " of", "re", "of ", "t ", ", ", "is", "u", "at",
            "   ", "n ", "or", "which", "f", "m", "as", "it", "that", "\n", "was", "en",
            "  ", " w", "es", " an", " i", "f ", "g", "p", "nd", " s", "nd ", "ed ",
            "w", "ed", "http://", "https://", "for", "te", "ing", "y ", "The", " c", "ti", "r ", "his",
            "st", " in", "ar", "nt", ",", " to", "y", "ng", " h", "with", "le", "al", "to ",
            "b", "ou", "be", "were", " b", "se", "o ", "ent", "ha", "ng ", "their", "\"",
            "hi", "from", " f", "in ", "de", "ion", "me", "v", ".", "ve", "all", "re ",
            "ri", "ro", "is ", "co", "f t", "are", "ea", ". ", "her", " m", "er ", " p",
            "es ", "by", "they", "di", "ra", "ic", "not", "s, ", "d t", "at ", "ce", "la",
            "h ", "ne", "as ", "tio", "on ", "n t", "io", "we", " a ", "om", ", a", "s o",
            "ur", "li", "ll", "ch", "had", "this", "e t", "g ", " wh", "ere",
            " co", "e o", "a ", "us", " d", "ss", " be", " e","@",
            "s a", "ma", "one", "t t", "or ", "but", "el", "so", "l ", "e s", "s,", "no",
            "ter", " wa", "iv", "ho", "e a", " r", "hat", "s t", "ns", "ch ", "wh", "tr",
            "ut", "/", "have", "ly ", "ta", " ha", " on", "tha", "-", " l", "ati", "en ",
            "pe", " re", "there", "ass", "si", " fo", "wa", "ec", "our", "who", "its", "z",
            "fo", "rs", "ot", "un", "im", "th ", "nc", "ate", "ver", "ad",
            " we", "ly", "ee", " n", "id", " cl", "ac", "il", "rt", " wi",
            "e, ", " it", "whi", " ma", "ge", "x", "e c", "men", ".com"
        };

        private readonly byte*[] _termsTableBytes;

        private readonly byte*[][] _hashTable;
        private readonly string[] _termsTable;

        private readonly int _maxTermSize;
        private readonly int _maxVerbatimLen;

        public static Smaz Instance = new Smaz(DefaultTermsTable);

        public Smaz(string[] termsTable)
        {
            _termsTable = termsTable;
            if (termsTable.Length + 8 > byte.MaxValue)
                throw new InvalidOperationException("Too many terms defined");

            _termsTableBytes = new byte*[termsTable.Length];
            _maxVerbatimLen = Math.Min(byte.MaxValue - termsTable.Length, 48);
            _hashTable = new byte*[byte.MaxValue][];
            for (int i = 0; i < termsTable.Length; i++)
            {
                var byteCount = Encodings.Utf8.GetByteCount(termsTable[i]);
                if (byteCount > byte.MaxValue)
                    throw new InvalidOperationException("Term " + termsTable[i] + " is too big");
                var ptr = (byte*)NativeMemory.AllocateMemory(byteCount + 2);
                _termsTableBytes[i] = ptr;
                ptr[0] = (byte)byteCount;
                fixed (char* pChars = termsTable[i])
                {
                    var bytes = Encodings.Utf8.GetBytes(pChars, termsTable[i].Length, ptr + 1, byteCount);
                    if (bytes != byteCount)
                        throw new InvalidOperationException("Bug, UTF8 encoding mismatch for GetByteCount and GetBytes for " + termsTable[i]);
                }
                ptr[byteCount + 1] = (byte)i;

                _maxTermSize = Math.Max(_maxTermSize, byteCount);

                AddToHash(ptr, byteCount);
            }
            var empty = new byte*[0];

            for (int i = 0; i < _hashTable.Length; i++)
            {
                if (_hashTable[i] == null)
                    _hashTable[i] = empty;
            }
        }

        private void AddToHash(byte* ptr, int byteCount)
        {
            int h = ptr[1] << 3;
            AddToHash(h, ptr);
            if (byteCount == 1)
                return;
            h += ptr[2];
            AddToHash(h, ptr);
            if (byteCount == 2)
                return;
            h ^= ptr[3];
            AddToHash(h, ptr);
        }

        private void AddToHash(int hash, byte* buffer)
        {
            var index = hash % _hashTable.Length;
            if (_hashTable[index] == null)
            {
                _hashTable[index] = new[] { buffer };
                return;
            }
            var newBuffer = new byte*[_hashTable[index].Length + 1];
            for (int i = 0; i < _hashTable[index].Length; i++)
            {
                newBuffer[i] = _hashTable[index][i];
            }
            newBuffer[newBuffer.Length - 1] = buffer;
            _hashTable[index] = newBuffer;
        }

        public int Decompress(byte* input, int inputLen, byte* output, int outputLen)
        {
            var outPos = 0;
            for (int i = 0; i < inputLen; i++)
            {
                var slot = input[i];
                if (slot >= _termsTable.Length)
                {
                    // verbatim entry
                    var len = slot - _termsTable.Length;
                    if (outPos + len > outputLen)
                        return 0;
                    Memory.Copy(output, input + i + 1, len);
                    outPos += len;
                    output += len;
                    i += len;
                }
                else
                {
                    var len = _termsTableBytes[slot][0];
                    if (outPos + len > outputLen)
                        return 0;
                    Memory.Copy(output, _termsTableBytes[slot] + 1, len);
                    output += len;
                    outPos += len;
                }
            }
            return outPos;
        }

        private struct CompressionState
        {
            public int OutputPosition;
            public int VerbatimStart;
            public int VerbatimLength;
        }

        public int Compress(byte* input, byte* output, int inputLen, int outputLen)
        {
            // we use stackalloc here so we can send a single state parameter
            // to the Flush method, and not have to allocate a value on the heap
            var state = stackalloc CompressionState[1];
            int h1, h2 = 0, h3 = 0;
            for (int i = 0; i < inputLen; i++)
            {
                h1 = input[i] << 3;
                if (i + 1 < inputLen)
                    h2 = h1 + input[i + 1];
                if (i + 2 < inputLen)
                    h3 = h2 ^ input[i + 2];

                int size = _maxTermSize;
                if (i + size >= inputLen)
                    size = inputLen - i;
                var foundMatch = false;
                for (; size > 0 && foundMatch == false; size--)
                {
                    byte*[] slot;
                    switch (size)
                    {
                        case 1: slot = _hashTable[h1 % _hashTable.Length]; break;
                        case 2: slot = _hashTable[h2 % _hashTable.Length]; break;
                        default: slot = _hashTable[h3 % _hashTable.Length]; break;
                    }
                    if (slot == null)
                        continue;
                    for (int j = 0; j < slot.Length; j++)
                    {
                        var termLegnth = slot[j][0];
                        if (termLegnth != size ||
                            Memory.Compare(input + i, slot[j] + 1, size) != 0)
                        {
                            continue;
                        }
                        if (state->OutputPosition + state->VerbatimLength + 1 > outputLen)
                            return 0;
                        if (state->VerbatimLength > 0)
                        {
                            Flush(input, output, state);
                        }
                        output[state->OutputPosition++] = slot[j][termLegnth + 1]; // get the index to write there
                        state->VerbatimStart = i + termLegnth;
                        i += termLegnth - 1; // skip the length we just compressed

                        foundMatch = true;
                        break;
                    }
                }
                if (foundMatch == false)
                    state->VerbatimLength++;
            }
            if (state->OutputPosition + state->VerbatimLength > outputLen)
                return 0;
            Flush(input, output, state);
            return state->OutputPosition;
        }

        private void Flush(byte* input, byte* output, CompressionState* state)
        {
            var verbatimLength = state->VerbatimLength;
            while (verbatimLength > 0)
            {
                var len = Math.Min(_maxVerbatimLen - 1, verbatimLength);
                output[state->OutputPosition++] = (byte)(len + _termsTable.Length);
                Memory.Copy(output + state->OutputPosition, input + state->VerbatimStart, len);
                state->VerbatimStart += len;
                verbatimLength -= len;
                state->OutputPosition += len;
            }
            state->VerbatimLength = verbatimLength;
        }
    }
}

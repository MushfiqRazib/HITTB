--select * from naw_best where  "zoeknaam"='2V/DRACHTEN'

Update naw_best set relatievan = 'BO' where relatievan = lower('BOL');
Update naw_best set relatievan = 'DE' where relatievan = lower('DEV');
Update naw_best set relatievan = 'VI' where relatievan = lower('VIM');
Update naw_best set relatievan = 'EN' where relatievan = lower('ENS');

--select relatievan from naw_best 
--select * from naw_best

--select opmerking from naw_best where opmerking  is not null
--(a) nieuw (b) actief (c) info (d) leverancie (e) tip (f) uitzoeken (g) vervalle
-- ----------------
--(a) Geen (b) Klant (c) Leverancier (d) Offerte (e) Prospect (f) Suspect



--select bedrijftyp from personen where bedrijftyp is not null

Update personen set status =  'Prospect' where status = upper('NIEUW'); 
Update personen set status =  'Prospect' where status= upper('ACTIEF');
Update personen set status =  'Prospect' where status= upper('INFO');
Update personen set status =  'Prospect' where status= upper('TIP');
Update personen set status =  'Prospect' where status= upper('UITZOEKEN');

Update personen set status = 'Geen' where status = upper('VERVALLE');
Update personen set status = 'Leverancier' where status = upper('LEVERANCIE');

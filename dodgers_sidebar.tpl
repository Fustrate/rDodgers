## NL West Standings

Team|W|L|PCT|GB
:-:|:-:|:-:|:-:|:-:
{%- for team in standings %}
[]({{ team.subreddit }})|{{ team.wins }}|{{ team.losses }}|{{ team.percent }}|{{ team.games_back }}
{%- endfor %}

*Last Update: {{ now|datetime("%m/%d/%y at %I:%M %p") }}*

## Last 10 Games ({{ wins }}-{{ losses }})

Date|Team|Result
:-:|:-:|:-
{%- for game in recent_games|reverse %}
{{ game.date|datetime('%a %m/%d') }}|{{ game.where }} []({{ game.against }})|[
{%- if game.outcome == "W" -%}
	**{{ game.outcome }} _{{ game.score }}_**
{%- else -%}
	{{ game.outcome }} _{{ game.score }}_
{%- endif %}](http://losangeles.dodgers.mlb.com/mlb/scoreboard/index.jsp?c_id=la#date={{ game.date|datetime("%m/%d/%Y") }})
{%- endfor %}

## {{ now|datetime("%B") }} Schedule

 |Day|Team|Time(PT)|TV
:--|:--|:--:|:--|:--|:--
{% for line in calendar %}
{{- line -}}
{% endfor %}

## Official Links

* [Dodgers.com](http://www.dodgers.com)
* [Dodgers Facebook Page](https://www.facebook.com/Dodgers)
* [Dodgers Twitter Page](http://twitter.com/Dodgers)

## Dodger Twitter Accounts

* [Todd Coffey](http://twitter.com/ToddCoffey60)
* [Luis Cruz](http://twitter.com/CochitoCruz)
* [Adrian Gonzalez](http://twitter.com/adriangon28)
* [Dee Gordon](http://twitter.com/skinnyswag9)
* [A.J. Ellis](http://twitter.com/ajellis17)
* [Josh Beckett](http://twitter.com/beck19bb)
* [Javy Guerra](http://twitter.com/javyguerra54)
* [Jerry Hairston Jr.](http://twitter.com/Therealjhair)
* [Matt Kemp](http://twitter.com/therealmattkemp)
* [Clayton Kershaw](http://twitter.com/ClaytonKersh22)
* [Brandon League](http://twitter.com/BrandonLeague43)
* [Nick Punto](http://twitter.com/Shredderpunto)
* [Hanley Ramirez](http://twitter.com/HanleyRamirez)
* [Shane Victorino](http://twitter.com/ShaneVictorino)
* [Magic Johnson](http://twitter.com/MagicJohnson)
* [Tommy Lasorda](http://twitter.com/TommyLasorda)

## Dodger News

* [LA Times](http://www.latimes.com/sports/baseball/mlb/dodgers)
* [Yahoo Sports](http://sports.yahoo.com/mlb/teams/lad)
* [ESPN](http://espn.go.com/mlb/team/_/name/lad/los-angeles-dodgers)
* [ESPN LA](http://espn.go.com/blog/los-angeles/dodger-report)
* [Sports Illustrated](http://sportsillustrated.cnn.com/baseball/mlb/teams/los-angeles-dodgers)
* [Fox Sports](http://msn.foxsports.com/mlb/team/los-angeles-dodgers/71605?q=los-angeles-dodgers)
* [Prime Ticket](http://www.foxsportswest.com/pages/dodgers)
* [MLB Trade Rumors](http://www.mlbtraderumors.com/los_angeles_dodgers)

## Dodger Blogs

* [Dodgers Insider](http://dodgersinsider.mlblogs.com)
* [Dodger Thoughts](http://www.dodgerthoughts.com)
* [Vin Scully Is My Homeboy](http://www.vinscullyismyhomeboy.com)
* [True Blue LA](http://www.truebluela.com)
* [The Left Field Pavilion](http://www.thelfp.com/blog)
* [Dodger Films](http://dodgerfilms.mlblogs.com)
* [Chad Moriyama](http://www.chadmoriyama.com)
* [Mike Scioscia's Tragic Illness](http://mikesciosciastragicillness.com)
* [Dodgers Blue Heaven](http://www.dodgersblueheaven.com)

## Related subreddits

* [/r/baseball](/r/baseball/)
* [/r/lakers](/r/lakers/)
* [/r/laclippers](/r/laclippers/)
* [/r/lagalaxy](/r/lagalaxy/)
* [/r/losangeleskings](/r/losangeleskings/)
* [/r/losangeles](/r/losangeles/)
* [/r/CityOfLA](/r/CityOfLA/)

---

Dodger Alien by [Atraktape](http://www.reddit.com/u/Atraktape)
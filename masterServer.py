#!/usr/bin/env python
#
# Copyright 2007 Google Inc.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#

from datetime import datetime
from datetime import timedelta

from google.appengine.ext import webapp
from google.appengine.ext.webapp import util
from google.appengine.ext import db

class MasterServer(db.Model):
    externalIp = db.StringProperty()
    externalPort = db.IntegerProperty()
    internalIp = db.StringProperty()
    internalPort = db.IntegerProperty()
    useNat = db.BooleanProperty()
    guid = db.StringProperty()
    gameType = db.StringProperty()
    gameName = db.StringProperty()
    connectedPlayers = db.IntegerProperty()
    playerLimit = db.IntegerProperty()
    passwordProtected = db.BooleanProperty()
    comment = db.StringProperty()
    updated = db.DateTimeProperty(auto_now_add=True)
    
class Query(webapp.RequestHandler):
    def get(self):
        dt = datetime.now () - timedelta(seconds=30);
        olds = db.GqlQuery("SELECT * from MasterServer WHERE updated < :1", dt)
        for old in olds:
            old.delete()

        games = db.GqlQuery("SELECT * from MasterServer WHERE gameType = :1", 
                    self.request.get('gameType'))
        show = 0;
        for game in games:
            print "game.datetime = " + str(game.updated)
            if (show == 1):
                print ";"
            else:
                show = 1 
            if (game.useNat and game.externalIp == self.request.remote_addr):
                self.response.out.write(game.internalIp + ',' + str(game.internalPort) + ',')
            else:
                self.response.out.write(game.externalIp + ',' + str(game.externalPort) + ',')
            if (game.useNat):
                self.response.out.write('1,');
            else:
                self.response.out.write('0,');
            self.response.out.write(game.guid + ',' + game.gameType + ',' + game.gameName + ',')
            self.response.out.write(str(game.connectedPlayers) + ',' + str(game.playerLimit) + ',')
            if (game.passwordProtected):
                self.response.out.write('1,');
            else:
                self.response.out.write('0,');
            self.response.out.write(game.comment + ',0');

class RegisterHost(webapp.RequestHandler):
    def get(self):
        olds = db.GqlQuery("SELECT * from MasterServer WHERE gameType = :1 AND gameName = :2", 
            self.request.get('gameType'), self.request.get('gameName'))
        for old in olds:
            old.delete()

        host = MasterServer ()
        host.gameType = self.request.get('gameType')
        host.gameName = self.request.get('gameName')
        host.useNat = self.request.get('useNat') == '1'
        host.connectedPlayers = int(self.request.get('connectedPlayers'))
        host.playerLimit = int(self.request.get('playerLimit'))
        host.inernalIp = self.request.get('internalIp')
        host.inernalPort = int(self.request.get('internalPort'))
        host.externalIp = self.request.get('externalIp')
        host.externalPort = int(self.request.get('externalPort'))
        host.guid = self.request.get('guid')
        host.passwordProtected = self.request.get('passwordProtected') == '1'
        host.comment = self.request.get('comment')
        host.put()

class UnregisterHost(webapp.RequestHandler):
    def get(self):
        games = db.GqlQuery("SELECT * from MasterServer WHERE gameType = :1 AND gameName = :2", 
            self.request.get('gameType'), self.request.get('gameName'))
        for game in games:
            game.delete()

class UpdateHost(webapp.RequestHandler):
    def get(self):
        games = db.GqlQuery("SELECT * from MasterServer WHERE gameType = :1 AND gameName = :2", 
            self.request.get('gameType'), self.request.get('gameName'))
        for game in games:
            game.updated = datetime.now ()
            game.put()

class UpdatePlayers(webapp.RequestHandler):
    def get(self):
        games = db.GqlQuery("SELECT * from MasterServer WHERE gameType = :1 AND gameName = :2", 
            self.request.get('gameType'), self.request.get('gameName'))
        for game in games:
            game.connectedPlayers = self.request.get('connectedPlayers')
            game.put()

class MainHandler(webapp.RequestHandler):
    def get(self):
        self.response.out.write ('hello world')

def main():
    application = webapp.WSGIApplication([
        ('/masterserver/query', Query),
        ('/masterserver/registerhost', RegisterHost),
        ('/masterserver/unregisterhost', UnregisterHost),
        ('/masterserver/updatehost', UpdateHost),
        ('/masterserver/updateplayers', UpdatePlayers),
        ('/masterserver/', MainHandler)], debug=True)
    util.run_wsgi_app(application)


if __name__ == '__main__':
    main()

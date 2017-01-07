using Ganss.XSS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BracketPipe;
using System.IO;
using WebMarkupMin.Core;

namespace BracketPipe.Performance
{
  class Program
  {
    static void Main(string[] args)
    {
      var sanitizer = new HtmlSanitizer();
      var html = @"<script>alert('xss')</script><div onload=""alert('xss')"""
          + @"style=""background-color: test"">Test<img src=""test.gif"""
          + @"style=""background-image: url(javascript:alert('xss')); margin: 10px""></div>";
      var memStream = new MemoryStream(Encoding.UTF8.GetBytes(html));

      Stopwatch st;
      for (var j = 0; j < 6; j++)
      {
        st = Stopwatch.StartNew();
        for (var i = 0; i < 5000; i++)
        {
          var sanitized = sanitizer.Sanitize(html, "http://www.example.com");
        }
        Console.WriteLine("HtmlSanitizer {0} ms", st.ElapsedMilliseconds);

        st = Stopwatch.StartNew();
        for (var i = 0; i < 5000; i++)
        {
          var sanitized = Html.Sanitize(html);
        }
        Console.WriteLine("BracketPipe {0} ms", st.ElapsedMilliseconds);

        st = Stopwatch.StartNew();
        for (var i = 0; i < 5000; i++)
        {
          memStream.Position = 0;
          var sanitized = Html.Sanitize(memStream);
        }
        Console.WriteLine("BracketPipe {0} ms", st.ElapsedMilliseconds);
      }


      const string htmlInput = @"<!DOCTYPE html>
<html>
    <head>
        <meta charset=""utf-8"" />
        <title>The test document</title>
        <link href=""favicon.ico"" rel=""shortcut icon"" type=""image/x-icon"" />
        <meta name=""viewport"" content=""width=device-width"" />
        <link rel=""stylesheet"" type=""text/css"" href=""/Content/Site.css"" />
    </head>
    <body>
        <p>Lorem ipsum dolor sit amet...</p>

        <script src=""http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.9.1.min.js""></script>
        <script>
            (window.jquery) || document.write('<script src=""/Scripts/jquery-1.9.1.min.js""><\/script>');
        </script>
    </body>
</html>";
      memStream = new MemoryStream(Encoding.UTF8.GetBytes(htmlInput));

      var htmlMinifier = new HtmlMinifier();
      for (var j = 0; j < 5; j++)
      {
        st = Stopwatch.StartNew();
        for (var i = 0; i < 5000; i++)
        {
          var result = htmlMinifier.Minify(htmlInput);
        }
        Console.WriteLine("WebMarkupMin {0} ms", st.ElapsedMilliseconds);

        st = Stopwatch.StartNew();
        for (var i = 0; i < 5000; i++)
        {
          var result = Html.Minify(htmlInput);
        }
        Console.WriteLine("BracketPipe {0} ms", st.ElapsedMilliseconds);

        st = Stopwatch.StartNew();
        for (var i = 0; i < 5000; i++)
        {
          memStream.Position = 0;
          var result = Html.Minify(memStream);
        }
        Console.WriteLine("BracketPipe {0} ms", st.ElapsedMilliseconds);
      }

      var mdConverter = new Html2Markdown.Converter();
      var revConverter = new ReverseMarkdown.Converter();
      const string mdHtml = "<p>This is the second part of a two part series about building real-time web applications with server-sent events.</p>\r\n\r\n<ul>\r\n<li><a href=\"http://bayn.es/real-time-web-applications-with-server-sent-events-pt-1/\">Building Web Apps with Server-Sent Events - Part 1</a></li>\r\n</ul>\r\n\r\n<h2 id=\"reconnecting\">Reconnecting</h2>\r\n\r\n<p>In this post we are going to look at handling reconnection if the browser loses contact with the server. Thankfully the native JavaScript functionality for SSEs (the <a href=\"https://developer.mozilla.org/en-US/docs/Web/API/EventSource\">EventSource</a>) handles this natively. You just need to make sure that your server-side implementation supports the mechanism.</p>\r\n\r\n<p>When the server reconnects your SSE end point it will send a special HTTP header <code>Last-Event-Id</code> in the reconnection request. In the previous part of this blog series we looked at just sending events with the <code>data</code> component. Which looked something like this:-</p>\r\n\r\n<pre><code>data: The payload we are sending\\n\\n\r\n</code></pre>\r\n\r\n<p>Now while this is enough to make the events make it to your client-side implementation. We need more information to handle reconnection. To do this we need to add an event id to the output.</p>\r\n\r\n<p>E.g.</p>\r\n\r\n<pre><code>id: 1439887379635\\n\r\ndata: The payload we are sending\\n\\n\r\n</code></pre>\r\n\r\n<p>The important thing to understand here is that each event needs a unique identifier, so that the client can communicate back to the server (using the <code>Last-Event-Id</code> header) which was the last event it received on reconnection.</p>\r\n\r\n<h2 id=\"persistence\">Persistence</h2>\r\n\r\n<p>In our previous example we used <a href=\"http://redis.io/topics/pubsub\">Redis Pub/Sub</a> to inform <a href=\"https://nodejs.org/\">Node.js</a> that it needs to push a new SSE to the client. Redis Pub/Sub is a topic communication which means it will be delivered to all <em>connected clients</em>, and then it will be removed from the topic. So there is no persistence for when clients reconnect. To implement this we need to add a persistence layer and so in this demo I have chosen to use <a href=\"https://www.mongodb.org/\">MongoDB</a>.</p>\r\n\r\n<p>Essentially we will be pushing events into both Redis and MongoDB. Redis will still be our method of initiating an SSE getting sent to the browser, but we will also be be storing that event into MongoDB so we can query it on a reconnection to get anything we've missed.</p>\r\n\r\n<h2 id=\"thecode\">The Code</h2>\r\n\r\n<p>OK so let us look at how we can actually implement this.</p>\r\n\r\n<h3 id=\"updateserverevent\">Update ServerEvent</h3>\r\n\r\n<p>We need to update the ServerEvent object to support having an <code>id</code> for an event.</p>\r\n\r\n<pre><code>function ServerEvent(name) {\r\n    this.name = name || \"\";\r\n    this.data = \"\";\r\n};\r\n\r\nServerEvent.prototype.addData = function(data) {\r\n    var lines = data.split(/\\n/);\r\n\r\n    for (var i = 0; i &lt; lines.length; i++) {\r\n        var element = lines[i];\r\n        this.data += \"data:\" + element + \"\\n\";\r\n    }\r\n}\r\n\r\nServerEvent.prototype.payload = function() {\r\n    var payload = \"\";\r\n    if (this.name != \"\") {\r\n        payload += \"id: \" + this.name + \"\\n\";\r\n    }\r\n\r\n    payload += this.data;\r\n    return payload + \"\\n\";\r\n}\r\n</code></pre>\r\n\r\n<p>This is pretty straightforward string manipulation and won't impress anyone, but it is foundation for what will follow.</p>\r\n\r\n<h3 id=\"storeeventsinmongodb\">Store Events in MongoDB</h3>\r\n\r\n<p>We need to update the <code>post.js</code> code to also store new events in MongoDB.</p>\r\n\r\n<pre><code>app.put(\"/api/post-update\", function(req, res) {\r\n    var json = req.body;\r\n    json.timestamp = Date.now();\r\n\r\n    eventStorage.save(json).then(function(doc) {\r\n        dataChannel.publish(JSON.stringify(json));\r\n    }, errorHandling);\r\n\r\n    res.status(204).end();\r\n});\r\n</code></pre>\r\n\r\n<p>The <code>event-storage</code> module looks as follows:</p>\r\n\r\n<pre><code>var Q = require(\"q\"),\r\n    config = require(\"./config\"),\r\n    mongo = require(\"mongojs\"),\r\n    db = mongo(config.mongoDatabase),\r\n    collection = db.collection(config.mongoScoresCollection);\r\n\r\nmodule.exports.save = function(data) {\r\n    var deferred = Q.defer();\r\n    collection.save(data, function(err, doc){\r\n        if(err) {\r\n            deferred.reject(err);\r\n        }\r\n        else {\r\n            deferred.resolve(doc);\r\n        }\r\n    });\r\n\r\n    return deferred.promise;\r\n};\r\n</code></pre>\r\n\r\n<p>Here we are just using basic MongoDB commands to save a new event into the collection. Yep that is it, we are now additionally persisting the events so they can be retrieved later.</p>\r\n\r\n<h3 id=\"retrievingeventsonreconnection\">Retrieving Events on Reconnection</h3>\r\n\r\n<p>When an <code>EventSource</code> reconnects after a disconnection it passes a special header <code>Last-Event-Id</code>. So we need to look for that and return the events that got broadcast while the client was disconnected.</p>\r\n\r\n<pre><code>app.get(\"/api/updates\", function(req, res){\r\n    initialiseSSE(req, res);\r\n\r\n    if (typeof(req.headers[\"last-event-id\"]) != \"undefined\") {\r\n        replaySSEs(req, res);\r\n    }\r\n});\r\n\r\nfunction replaySSEs(req, res) {\r\n    var lastId = req.headers[\"last-event-id\"];\r\n\r\n    eventStorage.findEventsSince(lastId).then(function(docs) {\r\n        for (var index = 0; index &lt; docs.length; index++) {\r\n            var doc = docs[index];\r\n            var messageEvent = new ServerEvent(doc.timestamp);\r\n            messageEvent.addData(doc.update);\r\n            outputSSE(req, res, messageEvent.payload());\r\n        }\r\n    }, errorHandling);\r\n};\r\n</code></pre>\r\n\r\n<p>What we are doing here is querying MongoDB for the events that were missed. We then iterate over them and output them to the browser.</p>\r\n\r\n<p>The code for querying MongoDB is as follows:</p>\r\n\r\n<pre><code>module.exports.findEventsSince = function(lastEventId) {\r\n    var deferred = Q.defer();\r\n\r\n    collection.find({\r\n        timestamp: {$gt: Number(lastEventId)}\r\n    })\r\n    .sort({timestamp: 1}, function(err, docs) {\r\n        if (err) {\r\n            deferred.reject(err);\r\n        }\r\n        else {\r\n            deferred.resolve(docs);\r\n        }\r\n    });\r\n\r\n    return deferred.promise;\r\n};\r\n</code></pre>\r\n\r\n<h2 id=\"testing\">Testing</h2>\r\n\r\n<p>To test this you will need to run both apps at the same time.</p>\r\n\r\n<pre><code>node app.js\r\n</code></pre>\r\n\r\n<p>and </p>\r\n\r\n<pre><code>node post.js\r\n</code></pre>\r\n\r\n<p>Once they are running open two browser windows <a href=\"http://localhost:8181/\">http://localhost:8181/</a> and <a href=\"http://localhost:8082/api/post-update\">http://localhost:8082/api/post-update</a></p>\r\n\r\n<p>Now you can post updates as before. If you stop <code>app.js</code> but continue posting events, when you restart <code>app.js</code> within 10 seconds the <code>EventSource</code> will reconnect. This will deliver all missed events.</p>\r\n\r\n<h2 id=\"conclusion\">Conclusion</h2>\r\n\r\n<p>This very simple code gives you a very elegant and powerful push architecture to create real-time apps.</p>\r\n\r\n<h3 id=\"improvements\">Improvements</h3>\r\n\r\n<p>A possible improvement would be to render the events from MongoDB server-side when the page is first output. Then we would get updates client-side as they are pushed to the browser.</p>\r\n\r\n<h3 id=\"download\">Download</h3>\r\n\r\n<p>If you want to play with this application you can fork or browse it on <a href=\"https://github.com/baynezy/RealtimeDemo/tree/part-2\">GitHub</a>.</p>";
      for (var j = 0; j < 5; j++)
      {
        st = Stopwatch.StartNew();
        for (var i = 0; i < 1000; i++)
        {
          var result = mdConverter.Convert(mdHtml);
        }
        Console.WriteLine("Html2Markdown {0} ms", st.ElapsedMilliseconds);

        st = Stopwatch.StartNew();
        for (var i = 0; i < 1000; i++)
        {
          var result = revConverter.Convert(mdHtml);
        }
        Console.WriteLine("ReverseMarkdown {0} ms", st.ElapsedMilliseconds);

        st = Stopwatch.StartNew();
        for (var i = 0; i < 1000; i++)
        {
          var result = Html.ToMarkdown(mdHtml);
        }
        Console.WriteLine("BracketPipe {0} ms", st.ElapsedMilliseconds);
      }


      Console.ReadLine();

    }
  }
}

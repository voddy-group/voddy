import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { FetchData } from './components/FetchData';
import { Counter } from './components/Counter';
import Setup from "./components/Setup/Setup";
import Search from "./components/Search/Search";
import Streams from "./components/Streams/Streams";
import Streamer from "./components/Streams/Streamer";
import './custom.css'

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/counter' component={Counter} />
        <Route path='/fetch-data' component={FetchData} />
        <Route path='/setup' component={Setup}/>
        <Route path='/search' component={Search}/>
        <Route path='/streams' component={Streams}/>
        <Route path='/streamer/:id' component={Streamer}/>
      </Layout>
    );
  }
}

import React, { Component } from 'react';
import { Container } from 'reactstrap';
import NavMenu from './NavMenu';
import TopBar from "./TopBar";
import "./Layout.css"

export class Layout extends Component {
  static displayName = Layout.name;

  render () {
    return (
      <div>
          <TopBar />
        <NavMenu />
        <Container>
          {this.props.children}
        </Container>
      </div>
    );
  }
}

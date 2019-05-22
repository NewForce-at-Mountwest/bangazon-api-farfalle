import React, {Component} from "react"
import { Link, withRouter } from "react-router-dom";
import "bootstrap/dist/css/bootstrap.min.css"

class Navbar extends {Component}{
    render(){
        return (
        <nav className="navbar navbar-light nav-orange flex-md-nowrap p-0 shadow">

        <React.Fragment>
            <ul className="nav nav-pills nav-fill decNav">
                <li className="nav-item">
                    <Link className="nav-link" to="/">Employees</Link>
                </li>
                <li className="nav-item">
                    <Link className="nav-link" to="/computers">Computers</Link>
                </li>
                <li className="nav-item">
                    <Link className="nav-link" to="/departments">Departments</Link>
                </li>
                <li className="nav-item">
                    <Link className="nav-link" to="/trainingprograms">Departments</Link>
                </li>
                    </ul>

        </React.Fragment>
        </nav>
      );
    }
}

export default withRouter(Navbar);
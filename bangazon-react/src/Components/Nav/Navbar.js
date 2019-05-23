import React, {Component} from "react"
import { Link, withRouter } from "react-router-dom";
import "bootstrap/dist/css/bootstrap.min.css"


class NavBar extends Component {


    render(){
        return (

        <React.Fragment>
            <ul className="nav nav-pills nav-fill decNav">

                <li className="nav-item">
                    <Link className="nav-link" to="/">Employees</Link>
                </li>
                <li className="nav-item">
                    <Link className="nav-link" to="/computers">Computers</Link>
                </li>
                <li className="nav-item">
                    <Link className="nav-link" to="/departments">Department</Link>
                </li>
                <li className="nav-item">
                    <Link className="nav-link" to="/trainingprograms">Training Programs</Link>
                </li>
                    </ul>

        </React.Fragment>

        )
    }
   }



    export default withRouter(NavBar);
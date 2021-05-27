import loadable from '@loadable/component';
import {Loading} from "../shared/components/Loading";


// Primary routes
export const Home = loadable(() => import('./Home/Home'), {
  LoadingComponent: Loading
});

export const Dashboard = loadable(() => import('./Dashboard/Dashboard'), {
  LoadingComponent: Loading
});
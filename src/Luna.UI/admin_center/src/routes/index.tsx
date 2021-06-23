import loadable from '@loadable/component';
import {Loading} from "../shared/components/Loading";


// Primary routes
export const Home = loadable(() => import('./Home/Home'), {
  LoadingComponent: Loading
});

export const Dashboard = loadable(() => import('./Dashboard/Dashboard'), {
  LoadingComponent: Loading
});

export const Reports = loadable(() => import('./Reports/Reports'), {
  LoadingComponent: Loading
});

export const Support = loadable(() => import('./Support/Support'), {
  LoadingComponent: Loading
});

export const Settings = loadable(() => import('./Settings/Settings'), {
  LoadingComponent: Loading
});


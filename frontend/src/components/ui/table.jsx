import * as React from "react";

const Table = ({ children, className, ...props }) => (
  <table className={className} {...props}>
    {children}
  </table>
);

export { Table };

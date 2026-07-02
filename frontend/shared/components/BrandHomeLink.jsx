import { Link } from 'react-router-dom';

export function BrandHomeLink({
  to,
  subtitle,
  subtitleClassName = 'brand-subtitle',
  className = 'brand-lockup',
  ariaLabel = 'Ir al inicio',
}) {
  return (
    <Link aria-label={ariaLabel} className={`${className} brand-home-link`} to={to}>
      <img
        alt="Sinergia"
        className="brand-logo"
        onError={(event) => { event.currentTarget.style.display = 'none'; }}
        src="/Logo_Sinergia.png"
      />
      <div>
        <h1>Sinergia</h1>
        {subtitle && <p className={subtitleClassName}>{subtitle}</p>}
      </div>
    </Link>
  );
}

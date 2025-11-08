/**
 * Datos de ubicaciones de Perú
 * Estructura: Departamento -> Provincias -> Distritos
 */

export interface District {
  name: string;
}

export interface Province {
  name: string;
  districts: District[];
}

export interface Department {
  name: string;
  provinces: Province[];
}

export const PERU_LOCATIONS: Department[] = [
  {
    name: 'Amazonas',
    provinces: [
      { name: 'Chachapoyas', districts: [{ name: 'Chachapoyas' }, { name: 'Asunción' }, { name: 'Balsas' }] },
      { name: 'Bagua', districts: [{ name: 'Bagua' }, { name: 'Aramango' }, { name: 'Copallín' }] }
    ]
  },
  {
    name: 'Áncash',
    provinces: [
      { name: 'Huaraz', districts: [{ name: 'Huaraz' }, { name: 'Cochabamba' }, { name: 'Colcabamba' }] },
      { name: 'Chimbote', districts: [{ name: 'Chimbote' }, { name: 'Cáceres del Perú' }, { name: 'Coishco' }] }
    ]
  },
  {
    name: 'Apurímac',
    provinces: [
      { name: 'Abancay', districts: [{ name: 'Abancay' }, { name: 'Chacoche' }, { name: 'Circa' }] },
      { name: 'Andahuaylas', districts: [{ name: 'Andahuaylas' }, { name: 'Andarapa' }, { name: 'Chiara' }] }
    ]
  },
  {
    name: 'Arequipa',
    provinces: [
      { name: 'Arequipa', districts: [{ name: 'Arequipa' }, { name: 'Alto Selva Alegre' }, { name: 'Cayma' }, { name: 'Cerro Colorado' }, { name: 'Characato' }, { name: 'Chiguata' }, { name: 'Jacobo Hunter' }, { name: 'La Joya' }, { name: 'Mariano Melgar' }, { name: 'Miraflores' }, { name: 'Mollebaya' }, { name: 'Paucarpata' }, { name: 'Pocsi' }, { name: 'Polobaya' }, { name: 'Quequeña' }, { name: 'Sabandía' }, { name: 'Sachaca' }, { name: 'San Juan de Siguas' }, { name: 'San Juan de Tarucani' }, { name: 'Santa Isabel de Siguas' }, { name: 'Santa Rita de Siguas' }, { name: 'Socabaya' }, { name: 'Tiabaya' }, { name: 'Uchumayo' }, { name: 'Vitor' }, { name: 'Yanahuara' }, { name: 'Yarabamba' }, { name: 'Yura' }] },
      { name: 'Camaná', districts: [{ name: 'Camaná' }, { name: 'José María Quimper' }, { name: 'Mariano Nicolás Valcárcel' }] }
    ]
  },
  {
    name: 'Ayacucho',
    provinces: [
      { name: 'Huamanga', districts: [{ name: 'Ayacucho' }, { name: 'Acocro' }, { name: 'Acos Vinchos' }] },
      { name: 'Huanta', districts: [{ name: 'Huanta' }, { name: 'Ayahuanco' }, { name: 'Huamanguilla' }] }
    ]
  },
  {
    name: 'Cajamarca',
    provinces: [
      { name: 'Cajamarca', districts: [{ name: 'Cajamarca' }, { name: 'Asunción' }, { name: 'Chetilla' }, { name: 'Cospan' }, { name: 'Encañada' }, { name: 'Jesús' }, { name: 'Llacanora' }, { name: 'Los Baños del Inca' }, { name: 'Magdalena' }, { name: 'Matara' }, { name: 'Namora' }, { name: 'San Juan' }] },
      { name: 'Jaén', districts: [{ name: 'Jaén' }, { name: 'Bellavista' }, { name: 'Chontali' }] }
    ]
  },
  {
    name: 'Callao',
    provinces: [
      { name: 'Callao', districts: [{ name: 'Callao' }, { name: 'Bellavista' }, { name: 'Carmen de la Legua Reynoso' }, { name: 'La Perla' }, { name: 'La Punta' }, { name: 'Ventanilla' }] }
    ]
  },
  {
    name: 'Cusco',
    provinces: [
      { name: 'Cusco', districts: [{ name: 'Cusco' }, { name: 'Ccorca' }, { name: 'Poroy' }, { name: 'San Jerónimo' }, { name: 'San Sebastian' }, { name: 'Santiago' }, { name: 'Saylla' }, { name: 'Wanchaq' }] },
      { name: 'Sicuani', districts: [{ name: 'Sicuani' }, { name: 'Checacupe' }, { name: 'Combapata' }] }
    ]
  },
  {
    name: 'Huancavelica',
    provinces: [
      { name: 'Huancavelica', districts: [{ name: 'Huancavelica' }, { name: 'Acobambilla' }, { name: 'Acoria' }] }
    ]
  },
  {
    name: 'Huánuco',
    provinces: [
      { name: 'Huánuco', districts: [{ name: 'Huánuco' }, { name: 'Amarilis' }, { name: 'Chinchao' }] }
    ]
  },
  {
    name: 'Ica',
    provinces: [
      { name: 'Ica', districts: [{ name: 'Ica' }, { name: 'La Tinguiña' }, { name: 'Los Aquijes' }, { name: 'Ocucaje' }, { name: 'Pachacutec' }, { name: 'Parcona' }, { name: 'Pueblo Nuevo' }, { name: 'Salas' }, { name: 'San José de Los Molinos' }, { name: 'San Juan Bautista' }, { name: 'Santiago' }, { name: 'Subtanjalla' }, { name: 'Tate' }, { name: 'Yauca del Rosario' }] },
      { name: 'Chincha', districts: [{ name: 'Chincha Alta' }, { name: 'Alto Laran' }, { name: 'Chavin' }] }
    ]
  },
  {
    name: 'Junín',
    provinces: [
      { name: 'Huancayo', districts: [{ name: 'Huancayo' }, { name: 'Carhuacallanga' }, { name: 'Chacapampa' }, { name: 'Chicche' }, { name: 'Chilca' }, { name: 'Chongos Alto' }, { name: 'Chupuro' }, { name: 'Colca' }, { name: 'Cullhuas' }, { name: 'El Tambo' }, { name: 'Huacrapuquio' }, { name: 'Hualhuas' }, { name: 'Huancan' }, { name: 'Huasicancha' }, { name: 'Huayucachi' }, { name: 'Ingenio' }, { name: 'Pariahuanca' }, { name: 'Pilcomayo' }, { name: 'Pucara' }, { name: 'Quichuay' }, { name: 'Quilcas' }, { name: 'San Agustín' }, { name: 'San Jerónimo de Tunán' }, { name: 'Saño' }, { name: 'Sapallanga' }, { name: 'Sicaya' }, { name: 'Santo Domingo de Acobamba' }, { name: 'Viques' }] },
      { name: 'Chanchamayo', districts: [{ name: 'La Merced' }, { name: 'Chanchamayo' }, { name: 'Perené' }] }
    ]
  },
  {
    name: 'La Libertad',
    provinces: [
      { name: 'Trujillo', districts: [{ name: 'Trujillo' }, { name: 'El Porvenir' }, { name: 'Florencia de Mora' }, { name: 'Huanchaco' }, { name: 'La Esperanza' }, { name: 'Laredo' }, { name: 'Moche' }, { name: 'Poroto' }, { name: 'Salaverry' }, { name: 'Simbal' }, { name: 'Victor Larco Herrera' }] },
      { name: 'Chepén', districts: [{ name: 'Chepén' }, { name: 'Pacanga' }, { name: 'Pueblo Nuevo' }] }
    ]
  },
  {
    name: 'Lambayeque',
    provinces: [
      { name: 'Chiclayo', districts: [{ name: 'Chiclayo' }, { name: 'Chongoyape' }, { name: 'Eten' }, { name: 'Eten Puerto' }, { name: 'José Leonardo Ortiz' }, { name: 'La Victoria' }, { name: 'Lagunas' }, { name: 'Monsefú' }, { name: 'Nueva Arica' }, { name: 'Oyotún' }, { name: 'Picsi' }, { name: 'Pimentel' }, { name: 'Reque' }, { name: 'Santa Rosa' }, { name: 'Saña' }, { name: 'Cayalti' }, { name: 'Patapo' }, { name: 'Pomalca' }, { name: 'Pucalá' }, { name: 'Tumán' }] },
      { name: 'Lambayeque', districts: [{ name: 'Lambayeque' }, { name: 'Chochope' }, { name: 'Illimo' }] }
    ]
  },
  {
    name: 'Lima',
    provinces: [
      { name: 'Lima', districts: [{ name: 'Lima' }, { name: 'Ancón' }, { name: 'Ate' }, { name: 'Barranco' }, { name: 'Breña' }, { name: 'Carabayllo' }, { name: 'Chaclacayo' }, { name: 'Chorrillos' }, { name: 'Cieneguilla' }, { name: 'Comas' }, { name: 'El Agustino' }, { name: 'Independencia' }, { name: 'Jesús María' }, { name: 'La Molina' }, { name: 'La Victoria' }, { name: 'Lince' }, { name: 'Los Olivos' }, { name: 'Lurigancho' }, { name: 'Lurín' }, { name: 'Magdalena del Mar' }, { name: 'Miraflores' }, { name: 'Pachacamac' }, { name: 'Pucusana' }, { name: 'Pueblo Libre' }, { name: 'Puente Piedra' }, { name: 'Punta Hermosa' }, { name: 'Punta Negra' }, { name: 'Rímac' }, { name: 'San Bartolo' }, { name: 'San Borja' }, { name: 'San Isidro' }, { name: 'San Juan de Lurigancho' }, { name: 'San Juan de Miraflores' }, { name: 'San Luis' }, { name: 'San Martín de Porres' }, { name: 'San Miguel' }, { name: 'Santa Anita' }, { name: 'Santa María del Mar' }, { name: 'Santa Rosa' }, { name: 'Santiago de Surco' }, { name: 'Surquillo' }, { name: 'Villa El Salvador' }, { name: 'Villa María del Triunfo' }] },
      { name: 'Cañete', districts: [{ name: 'San Vicente de Cañete' }, { name: 'Asia' }, { name: 'Calango' }] },
      { name: 'Huaral', districts: [{ name: 'Huaral' }, { name: 'Atavillos Alto' }, { name: 'Atavillos Bajo' }] }
    ]
  },
  {
    name: 'Loreto',
    provinces: [
      { name: 'Maynas', districts: [{ name: 'Iquitos' }, { name: 'Alto Nanay' }, { name: 'Fernando Lores' }] }
    ]
  },
  {
    name: 'Madre de Dios',
    provinces: [
      { name: 'Tambopata', districts: [{ name: 'Tambopata' }, { name: 'Inambari' }, { name: 'Las Piedras' }] }
    ]
  },
  {
    name: 'Moquegua',
    provinces: [
      { name: 'Mariscal Nieto', districts: [{ name: 'Moquegua' }, { name: 'Carumas' }, { name: 'Cuchumbaya' }] }
    ]
  },
  {
    name: 'Pasco',
    provinces: [
      { name: 'Pasco', districts: [{ name: 'Chaupimarca' }, { name: 'Huachon' }, { name: 'Huariaca' }] }
    ]
  },
  {
    name: 'Piura',
    provinces: [
      { name: 'Piura', districts: [{ name: 'Piura' }, { name: 'Castilla' }, { name: 'Catacaos' }, { name: 'Cura Mori' }, { name: 'El Tallan' }, { name: 'La Arena' }, { name: 'La Unión' }, { name: 'Las Lomas' }, { name: 'Tambo Grande' }] },
      { name: 'Sullana', districts: [{ name: 'Sullana' }, { name: 'Bellavista' }, { name: 'Ignacio Escudero' }] }
    ]
  },
  {
    name: 'Puno',
    provinces: [
      { name: 'Puno', districts: [{ name: 'Puno' }, { name: 'Acora' }, { name: 'Amantani' }] }
    ]
  },
  {
    name: 'San Martín',
    provinces: [
      { name: 'Moyobamba', districts: [{ name: 'Moyobamba' }, { name: 'Calzada' }, { name: 'Habana' }] }
    ]
  },
  {
    name: 'Tacna',
    provinces: [
      { name: 'Tacna', districts: [{ name: 'Tacna' }, { name: 'Alto de la Alianza' }, { name: 'Calana' }, { name: 'Ciudad Nueva' }, { name: 'Inclan' }, { name: 'Pachia' }, { name: 'Palca' }, { name: 'Pocollay' }, { name: 'Sama' }, { name: 'Coronel Gregorio Albarracín Lanchipa' }] }
    ]
  },
  {
    name: 'Tumbes',
    provinces: [
      { name: 'Tumbes', districts: [{ name: 'Tumbes' }, { name: 'Corrales' }, { name: 'La Cruz' }] }
    ]
  },
  {
    name: 'Ucayali',
    provinces: [
      { name: 'Coronel Portillo', districts: [{ name: 'Callería' }, { name: 'Campoverde' }, { name: 'Iparia' }] }
    ]
  }
];

/**
 * Obtiene todos los departamentos
 */
export function getDepartments(): string[] {
  return PERU_LOCATIONS.map(dept => dept.name).sort();
}

/**
 * Obtiene las provincias de un departamento
 */
export function getProvincesByDepartment(departmentName: string): string[] {
  const department = PERU_LOCATIONS.find(d => d.name === departmentName);
  if (!department) return [];
  return department.provinces.map(p => p.name).sort();
}

/**
 * Obtiene los distritos de una provincia en un departamento
 */
export function getDistrictsByProvince(departmentName: string, provinceName: string): string[] {
  const department = PERU_LOCATIONS.find(d => d.name === departmentName);
  if (!department) return [];
  const province = department.provinces.find(p => p.name === provinceName);
  if (!province) return [];
  return province.districts.map(d => d.name).sort();
}

